using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public AdminRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Usuarios> Items, int Total)> ObtenerUsuariosAsync(FiltroAdminRequest filtro)
    {
        var query = _context.Usuarios.Include(u => u.Rol).Include(u => u.Pais).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.NombreUsuario))
            query = query.Where(u => u.NombreUsuario.Contains(filtro.NombreUsuario));
        if (!string.IsNullOrWhiteSpace(filtro.CorreoElectronico))
            query = query.Where(u => u.CorreoElectronico.Contains(filtro.CorreoElectronico));
        if (!string.IsNullOrWhiteSpace(filtro.Filtro))
        {
            var f = filtro.Filtro.ToLower();
            query = query.Where(u =>
                u.NombreUsuario.ToLower().Contains(f) || u.CorreoElectronico.ToLower().Contains(f));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(u => u.Estado == filtro.Estado);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.FechaRegistro)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Usuarios?> ObtenerUsuarioPorIdAsync(int usuarioId) =>
        await _context.Usuarios
            .Include(u => u.Rol).Include(u => u.Pais)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

    public async Task<Usuarios?> ObtenerUsuarioConOrdenesYOfertasAsync(int usuarioId) =>
        await _context.Usuarios
            .Include(u => u.OrdenesCompra.Where(o => o.Estado == "Activa" || o.Estado == "Parcial"))
            .ThenInclude(o => o.ParMoneda)
            .Include(u => u.OfertasVenta.Where(o => o.Estado == "Activa" || o.Estado == "Parcial"))
            .ThenInclude(o => o.ParMoneda)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

    public async Task<(List<AuditoriaAdministrativa> Items, int Total)> ObtenerAuditoriaAsync(
        FiltroAuditoriaRequest filtro)
    {
        var query = _context.AuditoriaAdministrativa
            .Include(a => a.Administrador).Include(a => a.UsuarioAfectado).AsQueryable();

        if (filtro.Desde.HasValue) query = query.Where(a => a.FechaHora >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) query = query.Where(a => a.FechaHora <= filtro.Hasta.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Administrador))
            query = query.Where(a => a.Administrador.NombreUsuario.Contains(filtro.Administrador));
        if (!string.IsNullOrWhiteSpace(filtro.UsuarioAfectado))
            query = query.Where(a => a.UsuarioAfectado.NombreUsuario.Contains(filtro.UsuarioAfectado));
        if (!string.IsNullOrWhiteSpace(filtro.TipoAccion) && filtro.TipoAccion != "Todos")
            query = query.Where(a => a.TipoAccion == filtro.TipoAccion);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.FechaHora)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync();
        return (items, total);
    }

    public async Task<List<ConfiguracionSistema>> ObtenerConfiguracionesAsync() =>
        await _context.ConfiguracionSistema.OrderBy(c => c.Clave).ToListAsync();

    public async Task<ConfiguracionSistema?> ObtenerConfiguracionPorIdAsync(int id) =>
        await _context.ConfiguracionSistema.FindAsync(id);

    public Task AgregarAuditoriaAsync(AuditoriaAdministrativa auditoria) { _context.AuditoriaAdministrativa.Add(auditoria); return Task.CompletedTask; }
    public Task AgregarRestriccionAsync(RestriccionesUsuario restriccion) { _context.RestriccionesUsuario.Add(restriccion); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
