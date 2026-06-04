using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IRutaConversionService
{
    Task<BusquedaRutaDto> BuscarRutaAsync(int usuarioId, BuscarRutaRequest request);
    Task<List<BusquedaRutaDto>> ObtenerMisBusquedasAsync(int usuarioId);
}
