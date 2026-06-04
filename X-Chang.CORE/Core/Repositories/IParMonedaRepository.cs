using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public interface IParMonedaRepository
{
    Task<ParesMoneda?> ObtenerPorIdAsync(int parMonedaId, bool incluirMonedas = false);
    Task<List<ParesMoneda>> ObtenerActivosAsync(bool incluirMonedas = false);
    Task<ParesMoneda?> ObtenerPorCodigosAsync(string origen, string destino);
    Task<List<HistoricoPreciosPar>> ObtenerHistoricoAsync(int parMonedaId, DateTime desde);
    Task<List<int>> ObtenerIdsEnHistorialUsuarioAsync(int usuarioId);
    Task GuardarCambiosAsync();
}
