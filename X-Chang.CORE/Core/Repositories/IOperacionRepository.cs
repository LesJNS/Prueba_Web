using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public interface IOperacionRepository
{
    Task<OperacionesInmediatas?> ObtenerPorIdAsync(int operacionId, bool incluirEjecuciones = false);
    Task<OperacionesInmediatas?> ObtenerPorIdYUsuarioAsync(int operacionId, int usuarioId, bool incluirEjecuciones = false);
    Task<List<OperacionesInmediatas>> ObtenerPorUsuarioAsync(int usuarioId);
    Task AgregarAsync(OperacionesInmediatas operacion);
    Task AgregarEjecucionAsync(OperacionInmediataEjecuciones ejecucion);
    Task GuardarCambiosAsync();
}
