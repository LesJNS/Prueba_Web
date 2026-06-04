using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class OrdenService : IOrdenService
{
    private readonly ExchangeDivisasDbContext _context;

    public OrdenService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrdenDto>> ObtenerMisOrdenesAsync(int usuarioId, FiltroOrdenesRequest filtro)
    {
        var query = _context.OrdenesCompra
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

        return new PagedResult<OrdenDto>(
            items.Select(o => MapOrdenDto(o, o.ParMoneda)).ToList(),
            total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<OrdenDto> ObtenerOrdenAsync(int usuarioId, int ordenId)
    {
        var orden = await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenId && o.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Orden no encontrada.");

        return MapOrdenDto(orden, orden.ParMoneda);
    }

    public async Task CancelarOrdenAsync(int usuarioId, int ordenId)
    {
        var orden = await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
            .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenId && o.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Orden no encontrada.");

        if (orden.Estado is "Cancelada" or "Completada")
            throw new InvalidOperationException($"La orden está {orden.Estado.ToLower()} y no puede cancelarse.");

        var montoReembolso = orden.CantidadPendiente * orden.PrecioUnitario;

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldoOrigen = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
            _context, billetera.BilleteraId, orden.ParMoneda.MonedaOrigenId);
        var saldoAnterior = saldoOrigen.SaldoDisponible;
        saldoOrigen.SaldoDisponible += montoReembolso;
        saldoOrigen.FechaActualizacion = DateTime.UtcNow;

        orden.Estado = "Cancelada";
        orden.FechaCancelacion = DateTime.UtcNow;
        orden.FechaActualizacion = DateTime.UtcNow;

        _context.CancelacionesOrdenOferta.Add(new CancelacionesOrdenOferta
        {
            UsuarioId = usuarioId,
            TipoOperacion = "OrdenCompra",
            OrdenCompraId = ordenId,
            ParMonedaId = orden.ParMonedaId,
            CantidadEjecutada = orden.CantidadObtenida,
            CantidadCancelada = orden.CantidadPendiente,
            MontoReembolsado = montoReembolso,
            FechaCancelacion = DateTime.UtcNow
        });

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = usuarioId,
            MonedaId = orden.ParMoneda.MonedaOrigenId,
            TipoMovimiento = "DevolucionOrden",
            Monto = montoReembolso,
            SaldoAnterior = saldoAnterior,
            SaldoPosterior = saldoOrigen.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OrdenCompra",
            ReferenciaId = ordenId
        });

        _context.HistorialTransacciones.Add(new HistorialTransacciones
        {
            UsuarioId = usuarioId,
            TipoOperacion = "CancelacionOrden",
            ReferenciaId = ordenId,
            ParMonedaId = orden.ParMonedaId,
            FechaHora = DateTime.UtcNow,
            Estado = "Cancelada"
        });

        await _context.SaveChangesAsync();
    }

    private static OrdenDto MapOrdenDto(OrdenesCompra o, ParesMoneda par) =>
        new(o.OrdenCompraId, o.ParMonedaId,
            par.MonedaOrigen.CodigoIso, par.MonedaDestino.CodigoIso,
            o.CantidadOriginal, o.CantidadObtenida, o.CantidadPendiente,
            o.PrecioUnitario, o.TotalComprometido, o.TotalEjecutado,
            o.Estado, o.FechaCreacion, o.FechaActualizacion);
}
