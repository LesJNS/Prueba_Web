using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Repositories;

public class OrdenRepository : IOrdenRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public OrdenRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<OrdenesCompra?> ObtenerPorIdAsync(int ordenId, bool incluirPar = false)
    {
        var query = _context.OrdenesCompra.AsQueryable();
        if (incluirPar)
            query = query
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino);
        return await query.FirstOrDefaultAsync(o => o.OrdenCompraId == ordenId);
    }

    public async Task<OrdenesCompra?> ObtenerPorIdYUsuarioAsync(int ordenId, int usuarioId, bool incluirPar = false)
    {
        var query = _context.OrdenesCompra.AsQueryable();
        if (incluirPar)
            query = query
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino);
        return await query.FirstOrDefaultAsync(o => o.OrdenCompraId == ordenId && o.UsuarioId == usuarioId);
    }

    public async Task<(List<OrdenesCompra> Items, int Total)> ObtenerPorUsuarioAsync(
        int usuarioId, FiltroOrdenesRequest filtro)
    {
        var query = _context.OrdenesCompra
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

    public async Task<List<NivelOrdenDto>> ObtenerNivelesCompraAsync(int parMonedaId, int limite = 20) =>
        await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(g.Key, g.Sum(o => o.CantidadPendiente), g.Count()))
            .OrderByDescending(n => n.Precio)
            .Take(limite)
            .ToListAsync();

    public async Task<List<LibroOrdenEntradaDto>> ObtenerEntradasCompraAsync(int parMonedaId, int limite = 10) =>
        await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .OrderByDescending(o => o.PrecioUnitario)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(o.OrdenCompraId, o.CantidadPendiente, o.PrecioUnitario, o.FechaCreacion))
            .ToListAsync();

    public async Task<List<OrdenesCompra>> ObtenerActivasPorParAsync(int parMonedaId, decimal? precioMinimo = null)
    {
        var query = _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa" && o.CantidadPendiente > 0);
        if (precioMinimo.HasValue) query = query.Where(o => o.PrecioUnitario >= precioMinimo.Value);
        return await query.OrderByDescending(o => o.PrecioUnitario).ThenBy(o => o.FechaCreacion).ToListAsync();
    }

    public Task AgregarAsync(OrdenesCompra orden) { _context.OrdenesCompra.Add(orden); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
