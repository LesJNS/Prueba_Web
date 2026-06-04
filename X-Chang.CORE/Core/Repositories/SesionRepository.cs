using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public class SesionRepository : ISesionRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public SesionRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<SesionesUsuario?> ObtenerActivaPorTokenAsync(string token) =>
        await _context.SesionesUsuario
            .FirstOrDefaultAsync(s => s.TokenSesion == token && s.Estado == "Activa");

    public async Task<SesionesUsuario?> ObtenerActivaPorTokenConUsuarioAsync(string token) =>
        await _context.SesionesUsuario
            .Include(s => s.Usuario).ThenInclude(u => u.Rol)
            .FirstOrDefaultAsync(s => s.TokenSesion == token && s.Estado == "Activa");

    public async Task<List<SesionesUsuario>> ObtenerActivasPorUsuarioAsync(int usuarioId) =>
        await _context.SesionesUsuario
            .Where(s => s.UsuarioId == usuarioId && s.Estado == "Activa")
            .ToListAsync();

    public Task AgregarAsync(SesionesUsuario sesion) { _context.SesionesUsuario.Add(sesion); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
