using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IAdminService
{
    Task<PagedResult<AdminUsuarioDto>> ObtenerUsuariosAsync(FiltroAdminRequest filtro);
    Task<AdminUsuarioDto> ObtenerUsuarioAsync(int usuarioId);
    Task CambiarEstadoUsuarioAsync(int adminId, CambiarEstadoUsuarioRequest request);
    Task RestringirUsuarioAsync(int adminId, RestringirUsuarioRequest request);
    Task HabilitarUsuarioAsync(int adminId, HabilitarUsuarioRequest request);
    Task<RestriccionDto> AplicarRestriccionAsync(int adminId, RestriccionRequest request);
    Task<PagedResult<AuditoriaDto>> ObtenerAuditoriaAsync(FiltroAuditoriaRequest filtro);
    Task<DashboardDto> ObtenerDashboardAsync(FiltroDashboardRequest filtro);
    Task<List<ConfiguracionDto>> ObtenerConfiguracionesAsync();
    Task<ConfiguracionDto> ActualizarConfiguracionAsync(int adminId, int configuracionId, ActualizarConfiguracionRequest request);
}
