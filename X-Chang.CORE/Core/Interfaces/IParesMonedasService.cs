using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IParesMonedasService
{
    Task<PagedResult<ParMonedasDto>> ObtenerParesAsync(FiltroParesRequest filtro, int? usuarioId);
    Task<ParMonedasDetalleDto> ObtenerParDetalleAsync(int parMonedaId, FiltroHistoricoRequest filtro);
    Task<GraficoPrincipalDto> ObtenerGraficoPrincipalAsync(int? usuarioId);
}
