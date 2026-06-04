using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IOfertaService
{
    Task<OfertaDto> CrearOfertaVentaAsync(int usuarioId, CrearOfertaRequest request);
    Task<PagedResult<OfertaDto>> ObtenerMisOfertasAsync(int usuarioId, FiltroOfertasRequest filtro);
    Task<OfertaDto> ObtenerOfertaAsync(int usuarioId, int ofertaId);
    Task CancelarOfertaAsync(int usuarioId, int ofertaId);
}
