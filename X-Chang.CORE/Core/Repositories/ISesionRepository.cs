using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public interface ISesionRepository
{
    Task<SesionesUsuario?> ObtenerActivaPorTokenAsync(string token);
    Task<SesionesUsuario?> ObtenerActivaPorTokenConUsuarioAsync(string token);
    Task<List<SesionesUsuario>> ObtenerActivasPorUsuarioAsync(int usuarioId);
    Task AgregarAsync(SesionesUsuario sesion);
    Task GuardarCambiosAsync();
}
