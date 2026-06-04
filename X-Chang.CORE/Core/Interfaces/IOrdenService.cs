using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IOrdenService
{
    Task<PagedResult<OrdenDto>> ObtenerMisOrdenesAsync(int usuarioId, FiltroOrdenesRequest filtro);
    Task<OrdenDto> ObtenerOrdenAsync(int usuarioId, int ordenId);
    Task CancelarOrdenAsync(int usuarioId, int ordenId);
}
