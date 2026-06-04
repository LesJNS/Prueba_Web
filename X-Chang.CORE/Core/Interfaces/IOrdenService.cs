using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IOrdenService
{
    Task<OrdenDto> CrearOrdenCompraAsync(int usuarioId, CrearOrdenRequest request);
    Task<List<OrdenDto>> ObtenerMisOrdenesAsync(int usuarioId);
    Task<OrdenDto> ObtenerOrdenAsync(int usuarioId, int ordenId);
    Task CancelarOrdenAsync(int usuarioId, int ordenId);
    Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId);
}
