using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IOrdenService
{
    Task<OrdenDto> CrearOrdenCompraAsync(int usuarioId, CrearOrdenRequest request);
    Task<PagedResult<OrdenDto>> ObtenerMisOrdenesAsync(int usuarioId, FiltroOrdenesRequest filtro);
    Task<OrdenDto> ObtenerOrdenAsync(int usuarioId, int ordenId);
    Task CancelarOrdenAsync(int usuarioId, int ordenId);
    Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId);
    Task<LibroOrdenesDetalleDto> ObtenerLibroOrdenesDetalleAsync(int parMonedaId, int limite = 10);
}
