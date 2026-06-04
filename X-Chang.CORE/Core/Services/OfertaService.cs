using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class OfertaService : IOfertaService
{
    private readonly ExchangeDivisasDbContext _context;

    public OfertaService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OfertaDto>> ObtenerMisOfertasAsync(int usuarioId, FiltroOfertasRequest filtro)
    {
        var query = _context.OfertasVenta
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId)
            .AsQueryable();

        if (filtro.Desde.HasValue)
            query = query.Where(o => o.FechaCreacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue)
            query = query.Where(o => o.FechaCreacion <= filtro.Hasta.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(o => o.Estado == filtro.Estado);

        query = query.OrderByDescending(o => o.FechaCreacion);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync();

        return new PagedResult<OfertaDto>(
            items.Select(o => MapOfertaDto(o, o.ParMoneda)).ToList(),
            total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<OfertaDto> ObtenerOfertaAsync(int usuarioId, int ofertaId)
    {
        var oferta = await _context.OfertasVenta
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o => o.OfertaVentaId == ofertaId && o.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Oferta no encontrada.");

        return MapOfertaDto(oferta, oferta.ParMoneda);
    }

    public async Task CancelarOfertaAsync(int usuarioId, int ofertaId)
    {
        var oferta = await _context.OfertasVenta
            .Include(o => o.ParMoneda)
            .FirstOrDefaultAsync(o => o.OfertaVentaId == ofertaId && o.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Oferta no encontrada.");

        if (oferta.Estado is "Cancelada" or "Completada")
            throw new InvalidOperationException($"La oferta está {oferta.Estado.ToLower()} y no puede cancelarse.");

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldoDestino = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
            _context, billetera.BilleteraId, oferta.ParMoneda.MonedaDestinoId);
        var saldoAnterior = saldoDestino.SaldoDisponible;
        saldoDestino.SaldoDisponible += oferta.CantidadPendiente;
        saldoDestino.FechaActualizacion = DateTime.UtcNow;

        oferta.Estado = "Cancelada";
        oferta.FechaCancelacion = DateTime.UtcNow;
        oferta.FechaActualizacion = DateTime.UtcNow;

        _context.CancelacionesOrdenOferta.Add(new CancelacionesOrdenOferta
        {
            UsuarioId = usuarioId,
            TipoOperacion = "OfertaVenta",
            OfertaVentaId = ofertaId,
            ParMonedaId = oferta.ParMonedaId,
            CantidadEjecutada = oferta.CantidadVendida,
            CantidadCancelada = oferta.CantidadPendiente,
            MontoReembolsado = oferta.CantidadPendiente,
            FechaCancelacion = DateTime.UtcNow
        });

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = usuarioId,
            MonedaId = oferta.ParMoneda.MonedaDestinoId,
            TipoMovimiento = "DevolucionOferta",
            Monto = oferta.CantidadPendiente,
            SaldoAnterior = saldoAnterior,
            SaldoPosterior = saldoDestino.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OfertaVenta",
            ReferenciaId = ofertaId
        });

        _context.HistorialTransacciones.Add(new HistorialTransacciones
        {
            UsuarioId = usuarioId,
            TipoOperacion = "CancelacionOferta",
            ReferenciaId = ofertaId,
            ParMonedaId = oferta.ParMonedaId,
            FechaHora = DateTime.UtcNow,
            Estado = "Cancelada"
        });

        await _context.SaveChangesAsync();
    }

    private static OfertaDto MapOfertaDto(OfertasVenta o, ParesMoneda par) =>
        new(o.OfertaVentaId, o.ParMonedaId,
            par.MonedaOrigen.CodigoIso, par.MonedaDestino.CodigoIso,
            o.CantidadOriginal, o.CantidadVendida, o.CantidadPendiente,
            o.PrecioUnitario, o.TotalEsperado, o.TotalRecibido,
            o.Estado, o.FechaCreacion, o.FechaActualizacion);
}
