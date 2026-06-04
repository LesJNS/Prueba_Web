using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
#pragma warning disable CS8620

namespace X_Chang.CORE.Repositories;

public class HistorialRepository : IHistorialRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public HistorialRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<(List<HistorialTransacciones> Items, int Total)> ObtenerPorUsuarioAsync(
        int usuarioId, FiltroHistorialRequest filtro)
    {
        var query = _context.HistorialTransacciones
            .Include(h => h.ParMoneda).ThenInclude(p => p!.MonedaOrigen)
            .Include(h => h.ParMoneda).ThenInclude(p => p!.MonedaDestino)
            .Include(h => h.Moneda)
            .Where(h => h.UsuarioId == usuarioId)
            .AsQueryable();

        if (filtro.Desde.HasValue) query = query.Where(h => h.FechaHora >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) query = query.Where(h => h.FechaHora <= filtro.Hasta.Value);
        if (!string.IsNullOrWhiteSpace(filtro.TipoOperacion)) query = query.Where(h => h.TipoOperacion == filtro.TipoOperacion);
        if (!string.IsNullOrWhiteSpace(filtro.Estado)) query = query.Where(h => h.Estado == filtro.Estado);

        query = query.OrderByDescending(h => h.FechaHora);
        var total = await query.CountAsync();
        var items = await query.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public async Task<(List<OrdenesCompra> Items, int Total)> ObtenerOrdenesAsync(
        int usuarioId, FiltroColumnaRequest filtro)
    {
        var q = _context.OrdenesCompra
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId);
        if (filtro.Desde.HasValue) q = q.Where(o => o.FechaCreacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(o => o.FechaCreacion <= filtro.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaCreacion);
        var total = await q.CountAsync();
        var items = await q.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public async Task<(List<OfertasVenta> Items, int Total)> ObtenerOfertasAsync(
        int usuarioId, FiltroColumnaRequest filtro)
    {
        var q = _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId);
        if (filtro.Desde.HasValue) q = q.Where(o => o.FechaCreacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(o => o.FechaCreacion <= filtro.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaCreacion);
        var total = await q.CountAsync();
        var items = await q.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public async Task<(List<OperacionesInmediatas> Items, int Total)> ObtenerComprasInmediatasAsync(
        int usuarioId, FiltroColumnaRequest filtro)
    {
        var q = _context.OperacionesInmediatas
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId && o.TipoOperacion == "Compra" && o.OperacionPadreId == null);
        if (filtro.Desde.HasValue) q = q.Where(o => o.FechaOperacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(o => o.FechaOperacion <= filtro.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaOperacion);
        var total = await q.CountAsync();
        var items = await q.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public async Task<(List<OperacionesInmediatas> Items, int Total)> ObtenerVentasInmediatasAsync(
        int usuarioId, FiltroColumnaRequest filtro)
    {
        var q = _context.OperacionesInmediatas
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.InverseOperacionPadre)
            .ThenInclude(sub => sub.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId && o.TipoOperacion == "Venta" && o.OperacionPadreId == null);
        if (filtro.Desde.HasValue) q = q.Where(o => o.FechaOperacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(o => o.FechaOperacion <= filtro.Hasta.Value);
        q = q.OrderByDescending(o => o.FechaOperacion);
        var total = await q.CountAsync();
        var items = await q.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public async Task<(List<Depositos> Items, int Total)> ObtenerDepositosAsync(
        int usuarioId, FiltroColumnaRequest filtro)
    {
        var q = _context.Depositos.Include(d => d.Moneda).Include(d => d.MetodoPago).Where(d => d.UsuarioId == usuarioId);
        if (filtro.Desde.HasValue) q = q.Where(d => d.FechaDeposito >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(d => d.FechaDeposito <= filtro.Hasta.Value);
        q = q.OrderByDescending(d => d.FechaDeposito);
        var total = await q.CountAsync();
        var items = await q.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public async Task<(List<Retiros> Items, int Total)> ObtenerRetirosAsync(
        int usuarioId, FiltroColumnaRequest filtro)
    {
        var q = _context.Retiros.Include(r => r.Moneda).Include(r => r.MetodoPago).Where(r => r.UsuarioId == usuarioId);
        if (filtro.Desde.HasValue) q = q.Where(r => r.FechaRetiro >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(r => r.FechaRetiro <= filtro.Hasta.Value);
        q = q.OrderByDescending(r => r.FechaRetiro);
        var total = await q.CountAsync();
        var items = await q.Skip((filtro.Pagina - 1) * filtro.TamanoPagina).Take(filtro.TamanoPagina).ToListAsync();
        return (items, total);
    }

    public Task AgregarAsync(HistorialTransacciones historial) { _context.HistorialTransacciones.Add(historial); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
