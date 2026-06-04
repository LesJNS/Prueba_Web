using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IOperacionInmediataService
{
    Task<OperacionInmediataDto> EjecutarOperacionAsync(int usuarioId, OperacionInmediataRequest request);
    Task<List<OperacionInmediataDto>> ObtenerMisOperacionesAsync(int usuarioId);
    Task<OperacionInmediataDto> ObtenerOperacionAsync(int usuarioId, int operacionId);
}
