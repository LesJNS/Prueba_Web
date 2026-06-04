using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public class OperacionRepository : IOperacionRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public OperacionRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<OperacionesInmediatas?> ObtenerPorIdAsync(int operacionId, bool incluirEjecuciones = false)
    {
        var query = _context.OperacionesInmediatas
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .AsQueryable();
        if (incluirEjecuciones)
            query = query.Include(o => o.OperacionInmediataEjecuciones).ThenInclude(oe => oe.Ejecucion);
        return await query.FirstOrDefaultAsync(o => o.OperacionInmediataId == operacionId);
    }

    public async Task<OperacionesInmediatas?> ObtenerPorIdYUsuarioAsync(
        int operacionId, int usuarioId, bool incluirEjecuciones = false)
    {
        var query = _context.OperacionesInmediatas
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .AsQueryable();
        if (incluirEjecuciones)
            query = query.Include(o => o.OperacionInmediataEjecuciones).ThenInclude(oe => oe.Ejecucion);
        return await query.FirstOrDefaultAsync(o =>
            o.OperacionInmediataId == operacionId && o.UsuarioId == usuarioId);
    }

    public async Task<List<OperacionesInmediatas>> ObtenerPorUsuarioAsync(int usuarioId) =>
        await _context.OperacionesInmediatas
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.FechaOperacion)
            .ToListAsync();

    public Task AgregarAsync(OperacionesInmediatas operacion) { _context.OperacionesInmediatas.Add(operacion); return Task.CompletedTask; }
    public Task AgregarEjecucionAsync(OperacionInmediataEjecuciones ejecucion) { _context.OperacionInmediataEjecuciones.Add(ejecucion); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
