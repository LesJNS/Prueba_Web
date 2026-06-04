using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public interface IUsuarioRepository
{
    Task<bool> ExisteEmailAsync(string email);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    Task<Usuarios?> ObtenerPorCredencialAsync(string identificador);
    Task<Usuarios?> ObtenerPorIdAsync(int usuarioId);
    Task<Usuarios?> ObtenerConPaisAsync(int usuarioId);
    Task<Roles?> ObtenerRolPorNombreAsync(string nombre);
    Task<Paises?> ObtenerPaisPorIdAsync(int paisId);
    Task AgregarAsync(Usuarios usuario);
    Task GuardarCambiosAsync();
}
