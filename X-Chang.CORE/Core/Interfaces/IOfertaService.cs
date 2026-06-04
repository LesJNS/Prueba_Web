using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IOfertaService
{
    Task<OfertaDto> CrearOfertaVentaAsync(int usuarioId, CrearOfertaRequest request);
    Task<List<OfertaDto>> ObtenerMisOfertasAsync(int usuarioId);
    Task<OfertaDto> ObtenerOfertaAsync(int usuarioId, int ofertaId);
    Task CancelarOfertaAsync(int usuarioId, int ofertaId);
}
