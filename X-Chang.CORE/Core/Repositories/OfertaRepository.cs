using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Repositories;

public class OfertaRepository : IOfertaRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public OfertaRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<OfertasVenta?> ObtenerPorIdAsync(int ofertaId, bool incluirPar = false)
    {
        var query = _context.OfertasVenta.AsQueryable();
        if (incluirPar)
            query = query
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino);
        return await query.FirstOrDefaultAsync(o => o.OfertaVentaId == ofertaId);
    }

    public async Task<OfertasVenta?> ObtenerPorIdYUsuarioAsync(int ofertaId, int usuarioId, bool incluirPar = false)
    {
        var query = _context.OfertasVenta.AsQueryable();
        if (incluirPar)
            query = query
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino);
        return await query.FirstOrDefaultAsync(o => o.OfertaVentaId == ofertaId && o.UsuarioId == usuarioId);
    }

    public async Task<(List<OfertasVenta> Items, int Total)> ObtenerPorUsuarioAsync(
        int usuarioId, FiltroOfertasRequest filtro)
    {
        var query = _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId)
            .AsQueryable();

        if (filtro.Desde.HasValue) query = query.Where(o => o.FechaCreacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) query = query.Where(o => o.FechaCreacion <= filtro.Hasta.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Estado)) query = query.Where(o => o.Estado == filtro.Estado);

        query = query.OrderByDescending(o => o.FechaCreacion);
        var total = await query.CountAsync();
        var items = await query.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public async Task<List<NivelOrdenDto>> ObtenerNivelesVentaAsync(int parMonedaId, int limite = 20) =>
        await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(g.Key, g.Sum(o => o.CantidadPendiente), g.Count()))
            .OrderBy(n => n.Precio)
            .Take(limite)
            .ToListAsync();

    public async Task<List<LibroOrdenEntradaDto>> ObtenerEntradasVentaAsync(int parMonedaId, int limite = 10) =>
        await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .OrderBy(o => o.PrecioUnitario)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(o.OfertaVentaId, o.CantidadPendiente, o.PrecioUnitario, o.FechaCreacion))
            .ToListAsync();

    public async Task<List<OfertasVenta>> ObtenerActivasPorParAsync(int parMonedaId, decimal? precioMaximo = null)
    {
        var query = _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa" && o.CantidadPendiente > 0);
        if (precioMaximo.HasValue) query = query.Where(o => o.PrecioUnitario <= precioMaximo.Value);
        return await query.OrderBy(o => o.PrecioUnitario).ThenBy(o => o.FechaCreacion).ToListAsync();
    }

    public Task AgregarAsync(OfertasVenta oferta) { _context.OfertasVenta.Add(oferta); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
