using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public class RutaRepository : IRutaRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public RutaRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<BusquedasRuta?> ObtenerConRutasAsync(int busquedaId) =>
        await _context.BusquedasRuta
            .Include(b => b.RutasConversion)
            .ThenInclude(r => r.RutaConversionSaltos).ThenInclude(s => s.MonedaOrigen)
            .Include(b => b.RutasConversion)
            .ThenInclude(r => r.RutaConversionSaltos).ThenInclude(s => s.MonedaDestino)
            .Include(b => b.RutasConversion).ThenInclude(r => r.MonedaInicial)
            .Include(b => b.RutasConversion).ThenInclude(r => r.MonedaFinal)
            .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaId);

    public async Task<List<BusquedasRuta>> ObtenerPorUsuarioAsync(int usuarioId, int limite = 50) =>
        await _context.BusquedasRuta
            .Include(b => b.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(b => b.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(b => b.UsuarioId == usuarioId)
            .OrderByDescending(b => b.FechaInicio)
            .Take(limite)
            .ToListAsync();

    public async Task<List<ParesMoneda>> ObtenerParesActivosAsync() =>
        await _context.ParesMoneda
            .Include(p => p.MonedaOrigen).Include(p => p.MonedaDestino)
            .Where(p => p.Activo)
            .ToListAsync();

    public Task AgregarBusquedaAsync(BusquedasRuta busqueda) { _context.BusquedasRuta.Add(busqueda); return Task.CompletedTask; }
    public Task AgregarRutaAsync(RutasConversion ruta) { _context.RutasConversion.Add(ruta); return Task.CompletedTask; }
    public Task AgregarSaltoAsync(RutaConversionSaltos salto) { _context.RutaConversionSaltos.Add(salto); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
