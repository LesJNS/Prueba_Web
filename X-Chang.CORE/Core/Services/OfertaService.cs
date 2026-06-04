using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class OfertaService : IOfertaService
{
    private readonly ExchangeDivisasDbContext _context;
    private readonly IMatchingService _matching;

    public OfertaService(ExchangeDivisasDbContext context, IMatchingService matching)
    {
        _context = context;
        _matching = matching;
    }

    public async Task<OfertaDto> CrearOfertaVentaAsync(int usuarioId, CrearOfertaRequest request)
    {
        if (request.Cantidad <= 0 || request.PrecioUnitario <= 0)
            throw new ArgumentException("Cantidad y precio deben ser mayores a cero.");

        var par = await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p => p.ParMonedaId == request.ParMonedaId)
            ?? throw new InvalidOperationException("Par de moneda no encontrado.");

        if (!par.Activo)
            throw new InvalidOperationException("El par de moneda está inactivo.");

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldoDestino = await _context.SaldosBilletera
            .FirstOrDefaultAsync(s =>
                s.BilleteraId == billetera.BilleteraId &&
                s.MonedaId == par.MonedaDestinoId);

        if (saldoDestino == null || saldoDestino.SaldoDisponible < request.Cantidad)
            throw new InvalidOperationException("Saldo insuficiente en la moneda a vender.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var saldoAnterior = saldoDestino.SaldoDisponible;
            saldoDestino.SaldoDisponible -= request.Cantidad;
            saldoDestino.FechaActualizacion = DateTime.UtcNow;

            var oferta = new OfertasVenta
            {
                UsuarioId = usuarioId,
                ParMonedaId = request.ParMonedaId,
                CantidadOriginal = request.Cantidad,
                CantidadVendida = 0,
                CantidadPendiente = request.Cantidad,
                PrecioUnitario = request.PrecioUnitario,
                TotalEsperado = request.Cantidad * request.PrecioUnitario,
                TotalRecibido = 0,
                Estado = "Activa",
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };
            _context.OfertasVenta.Add(oferta);
            await _context.SaveChangesAsync();

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaDestinoId,
                TipoMovimiento = "ReservaOferta",
                Monto = -request.Cantidad,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldoDestino.SaldoDisponible,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "OfertaVenta",
                ReferenciaId = oferta.OfertaVentaId
            });

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "OfertaVenta",
                ReferenciaId = oferta.OfertaVentaId,
                ParMonedaId = request.ParMonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Activa"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            await _matching.EjecutarMatchingOfertaAsync(oferta.OfertaVentaId);

            await _context.Entry(oferta).ReloadAsync();

            return MapOfertaDto(oferta, par);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
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
