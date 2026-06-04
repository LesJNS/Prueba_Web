using X_Chang.API.Models;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Repositories;

public interface IAdminRepository
{
    Task<(List<Usuarios> Items, int Total)> ObtenerUsuariosAsync(FiltroAdminRequest filtro);
    Task<Usuarios?> ObtenerUsuarioPorIdAsync(int usuarioId);
    Task<Usuarios?> ObtenerUsuarioConOrdenesYOfertasAsync(int usuarioId);
    Task<(List<AuditoriaAdministrativa> Items, int Total)> ObtenerAuditoriaAsync(FiltroAuditoriaRequest filtro);
    Task<List<ConfiguracionSistema>> ObtenerConfiguracionesAsync();
    Task<ConfiguracionSistema?> ObtenerConfiguracionPorIdAsync(int id);
    Task AgregarAuditoriaAsync(AuditoriaAdministrativa auditoria);
    Task AgregarRestriccionAsync(RestriccionesUsuario restriccion);
    Task GuardarCambiosAsync();
}
