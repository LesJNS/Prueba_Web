using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public class ParMonedaRepository : IParMonedaRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public ParMonedaRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<ParesMoneda?> ObtenerPorIdAsync(int parMonedaId, bool incluirMonedas = false)
    {
        var query = _context.ParesMoneda.AsQueryable();
        if (incluirMonedas)
            query = query.Include(p => p.MonedaOrigen).Include(p => p.MonedaDestino);
        return await query.FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId);
    }

    public async Task<List<ParesMoneda>> ObtenerActivosAsync(bool incluirMonedas = false)
    {
        var query = _context.ParesMoneda.Where(p => p.Activo);
        if (incluirMonedas)
            query = query.Include(p => p.MonedaOrigen).Include(p => p.MonedaDestino);
        return await query.ToListAsync();
    }

    public async Task<ParesMoneda?> ObtenerPorCodigosAsync(string origen, string destino) =>
        await _context.ParesMoneda
            .Include(p => p.MonedaOrigen).Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p =>
                p.MonedaOrigen.CodigoIso == origen &&
                p.MonedaDestino.CodigoIso == destino &&
                p.Activo);

    public async Task<List<HistoricoPreciosPar>> ObtenerHistoricoAsync(int parMonedaId, DateTime desde) =>
        await _context.HistoricoPreciosPar
            .Where(h => h.ParMonedaId == parMonedaId && h.FechaRegistro >= desde)
            .OrderBy(h => h.FechaRegistro)
            .ToListAsync();

    public async Task<List<int>> ObtenerIdsEnHistorialUsuarioAsync(int usuarioId)
    {
        var ids = await _context.HistorialTransacciones
            .Where(h => h.UsuarioId == usuarioId && h.ParMonedaId != null)
            .Select(h => h.ParMonedaId)
            .ToListAsync();
        return ids.Where(id => id.HasValue).Select(id => id!.Value).ToList();
    }

    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
