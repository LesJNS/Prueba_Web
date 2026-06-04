using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;
#pragma warning disable CS8620

namespace X_Chang.CORE.Services;

public class HistorialService : IHistorialService
{
    private readonly ExchangeDivisasDbContext _context;

    public HistorialService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<HistorialDto>> ObtenerHistorialAsync(
        int usuarioId, FiltroHistorialRequest filtro)
    {
        var query = _context.HistorialTransacciones
            .Include(h => h.ParMoneda)
            .ThenInclude(p => p!.MonedaOrigen)
            .Include(h => h.ParMoneda)
            .ThenInclude(p => p!.MonedaDestino)
            .Include(h => h.Moneda)
            .Where(h => h.UsuarioId == usuarioId);

        if (filtro.Desde.HasValue)
            query = query.Where(h => h.FechaHora >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(h => h.FechaHora <= filtro.Hasta.Value);

        if (!string.IsNullOrWhiteSpace(filtro.TipoOperacion))
            query = query.Where(h => h.TipoOperacion == filtro.TipoOperacion);

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(h => h.Estado == filtro.Estado);

        query = query.OrderByDescending(h => h.FechaHora);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(h => new HistorialDto(
                h.HistorialId,
                h.TipoOperacion,
                h.ReferenciaId,
                h.ParMoneda != null
                    ? h.ParMoneda.MonedaOrigen.CodigoIso + "/" + h.ParMoneda.MonedaDestino.CodigoIso
                    : null,
                h.Moneda != null ? h.Moneda.CodigoIso : null,
                h.FechaHora,
                h.Estado,
                h.MetodoEjecucion))
            .ToListAsync();

        return new PagedResult<HistorialDto>(items, total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<HistorialCompletoDto> ObtenerHistorialCompletoAsync(
        int usuarioId,
        FiltroColumnaRequest filtroOrdenes,
        FiltroColumnaRequest filtroOfertas,
        FiltroColumnaRequest filtroCompras,
        FiltroColumnaRequest filtroVentas,
        FiltroColumnaRequest filtroDepositos,
        FiltroColumnaRequest filtroRetiros)
    {
        var ordenes = await ObtenerOrdenesAsync(usuarioId, filtroOrdenes);
        var ofertas = await ObtenerOfertasAsync(usuarioId, filtroOfertas);
        var compras = await ObtenerComprasInmediatasAsync(usuarioId, filtroCompras);
        var ventas = await ObtenerVentasInmediatasAsync(usuarioId, filtroVentas);
        var depositos = await ObtenerDepositosHistorialAsync(usuarioId, filtroDepositos);
        var retiros = await ObtenerRetirosHistorialAsync(usuarioId, filtroRetiros);
        return new HistorialCompletoDto(ordenes, ofertas, compras, ventas, depositos, retiros);
    }

    private async Task<PagedResult<HistorialOrdenDto>> ObtenerOrdenesAsync(int usuarioId, FiltroColumnaRequest f)
    {
        var q = _context.OrdenesCompra
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId);
        if (f.Desde.HasValue) q = q.Where(o => o.FechaCreacion >= f.Desde.Value);
        if (f.Hasta.HasValue) q = q.Where(o => o.FechaCreacion <= f.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaCreacion);
        var total = await q.CountAsync();
        var items = await q.Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .Select(o => new HistorialOrdenDto(
                o.OrdenCompraId, o.FechaCreacion,
                o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
                o.CantidadOriginal, o.CantidadObtenida, o.CantidadPendiente,
                o.PrecioUnitario, o.TotalComprometido, o.TotalEjecutado, o.Estado))
            .ToListAsync();
        return new PagedResult<HistorialOrdenDto>(items, total, f.Pagina, f.TamanoPagina);
    }

    private async Task<PagedResult<HistorialOfertaDto>> ObtenerOfertasAsync(int usuarioId, FiltroColumnaRequest f)
    {
        var q = _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId);
        if (f.Desde.HasValue) q = q.Where(o => o.FechaCreacion >= f.Desde.Value);
        if (f.Hasta.HasValue) q = q.Where(o => o.FechaCreacion <= f.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaCreacion);
        var total = await q.CountAsync();
        var items = await q.Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .Select(o => new HistorialOfertaDto(
                o.OfertaVentaId, o.FechaCreacion,
                o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
                o.CantidadOriginal, o.CantidadVendida, o.CantidadPendiente,
                o.PrecioUnitario, o.TotalEsperado, o.TotalRecibido, o.Estado))
            .ToListAsync();
        return new PagedResult<HistorialOfertaDto>(items, total, f.Pagina, f.TamanoPagina);
    }

    private async Task<PagedResult<HistorialCompraInmediataDto>> ObtenerComprasInmediatasAsync(int usuarioId, FiltroColumnaRequest f)
    {
        var q = _context.OperacionesInmediatas
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId
                && o.TipoOperacion == "Compra"
                && o.OperacionPadreId == null);
        if (f.Desde.HasValue) q = q.Where(o => o.FechaOperacion >= f.Desde.Value);
        if (f.Hasta.HasValue) q = q.Where(o => o.FechaOperacion <= f.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaOperacion);
        var total = await q.CountAsync();
        var items = await q.Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .ToListAsync();
        var dtos = items.Select(o => MapCompraInmediata(o)).ToList();
        return new PagedResult<HistorialCompraInmediataDto>(dtos, total, f.Pagina, f.TamanoPagina);
    }

    private static HistorialCompraInmediataDto MapCompraInmediata(OperacionesInmediatas o)
    {
        var subs = o.InverseOperacionPadre.Count > 0
            ? o.InverseOperacionPadre.Select(MapCompraInmediata).ToList()
            : null;
        return new HistorialCompraInmediataDto(
            o.OperacionInmediataId, o.FechaOperacion,
            o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
            o.CantidadEjecutada, o.PrecioMinimo, o.PrecioMaximo, o.PrecioPromedio,
            o.TotalPagado, o.Estado, o.MetodoEjecucion,
            subs != null, subs);
    }

    private async Task<PagedResult<HistorialVentaInmediataDto>> ObtenerVentasInmediatasAsync(int usuarioId, FiltroColumnaRequest f)
    {
        var q = _context.OperacionesInmediatas
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId
                && o.TipoOperacion == "Venta"
                && o.OperacionPadreId == null);
        if (f.Desde.HasValue) q = q.Where(o => o.FechaOperacion >= f.Desde.Value);
        if (f.Hasta.HasValue) q = q.Where(o => o.FechaOperacion <= f.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaOperacion);
        var total = await q.CountAsync();
        var items = await q.Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .ToListAsync();
        var dtos = items.Select(o => MapVentaInmediata(o)).ToList();
        return new PagedResult<HistorialVentaInmediataDto>(dtos, total, f.Pagina, f.TamanoPagina);
    }

    private static HistorialVentaInmediataDto MapVentaInmediata(OperacionesInmediatas o)
    {
        var subs = o.InverseOperacionPadre.Count > 0
            ? o.InverseOperacionPadre.Select(MapVentaInmediata).ToList()
            : null;
        return new HistorialVentaInmediataDto(
            o.OperacionInmediataId, o.FechaOperacion,
            o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
            o.CantidadEjecutada, o.PrecioMinimo, o.PrecioMaximo, o.PrecioPromedio,
            o.TotalRecibido, o.Estado, o.MetodoEjecucion,
            subs != null, subs);
    }

    private async Task<PagedResult<HistorialDepositoDto>> ObtenerDepositosHistorialAsync(int usuarioId, FiltroColumnaRequest f)
    {
        var q = _context.Depositos
            .Include(d => d.Moneda)
            .Include(d => d.MetodoPago)
            .Where(d => d.UsuarioId == usuarioId);
        if (f.Desde.HasValue) q = q.Where(d => d.FechaDeposito >= f.Desde.Value);
        if (f.Hasta.HasValue) q = q.Where(d => d.FechaDeposito <= f.Hasta.Value);
        q = q.OrderByDescending(d => d.FechaDeposito);
        var total = await q.CountAsync();
        var items = await q.Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .Select(d => new HistorialDepositoDto(
                d.DepositoId, d.FechaDeposito, d.Moneda.CodigoIso,
                d.MontoDepositado, d.MetodoPago.Nombre,
                d.ComisionAplicada, d.TotalPagado, d.Estado))
            .ToListAsync();
        return new PagedResult<HistorialDepositoDto>(items, total, f.Pagina, f.TamanoPagina);
    }

    private async Task<PagedResult<HistorialRetiroDto>> ObtenerRetirosHistorialAsync(int usuarioId, FiltroColumnaRequest f)
    {
        var q = _context.Retiros
            .Include(r => r.Moneda)
            .Include(r => r.MetodoPago)
            .Where(r => r.UsuarioId == usuarioId);
        if (f.Desde.HasValue) q = q.Where(r => r.FechaRetiro >= f.Desde.Value);
        if (f.Hasta.HasValue) q = q.Where(r => r.FechaRetiro <= f.Hasta.Value);
        q = q.OrderByDescending(r => r.FechaRetiro);
        var total = await q.CountAsync();
        var items = await q.Skip((f.Pagina - 1) * f.TamanoPagina).Take(f.TamanoPagina)
            .Select(r => new HistorialRetiroDto(
                r.RetiroId, r.FechaRetiro, r.Moneda.CodigoIso,
                r.MontoRetirado, r.MetodoPago.Nombre,
                r.ComisionAplicada, r.MontoFinalRecibido, r.Estado))
            .ToListAsync();
        return new PagedResult<HistorialRetiroDto>(items, total, f.Pagina, f.TamanoPagina);
    }
}
