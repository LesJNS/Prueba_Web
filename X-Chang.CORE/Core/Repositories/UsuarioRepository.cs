using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public UsuarioRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteEmailAsync(string email) =>
        await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == email);

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario) =>
        await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);

    public async Task<Usuarios?> ObtenerPorCredencialAsync(string identificador) =>
        await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u =>
                u.CorreoElectronico == identificador || u.NombreUsuario == identificador);

    public async Task<Usuarios?> ObtenerPorIdAsync(int usuarioId) =>
        await _context.Usuarios.FindAsync(usuarioId);

    public async Task<Usuarios?> ObtenerConPaisAsync(int usuarioId) =>
        await _context.Usuarios
            .Include(u => u.Pais)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

    public async Task<Roles?> ObtenerRolPorNombreAsync(string nombre) =>
        await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);

    public async Task<Paises?> ObtenerPaisPorIdAsync(int paisId) =>
        await _context.Paises.FindAsync(paisId);

    public Task AgregarAsync(Usuarios usuario)
    {
        _context.Usuarios.Add(usuario);
        return Task.CompletedTask;
    }

    public async Task GuardarCambiosAsync() =>
        await _context.SaveChangesAsync();
}
