using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IHistorialService
{
    Task<PagedResult<HistorialDto>> ObtenerHistorialAsync(int usuarioId, FiltroHistorialRequest filtro);
    Task<HistorialCompletoDto> ObtenerHistorialCompletoAsync(
        int usuarioId,
        FiltroColumnaRequest filtroOrdenes,
        FiltroColumnaRequest filtroOfertas,
        FiltroColumnaRequest filtroCompras,
        FiltroColumnaRequest filtroVentas,
        FiltroColumnaRequest filtroDepositos,
        FiltroColumnaRequest filtroRetiros);
}
