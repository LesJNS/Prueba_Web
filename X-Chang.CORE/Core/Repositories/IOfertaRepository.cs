using X_Chang.API.Models;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Repositories;

public interface IOfertaRepository
{
    Task<OfertasVenta?> ObtenerPorIdAsync(int ofertaId, bool incluirPar = false);
    Task<OfertasVenta?> ObtenerPorIdYUsuarioAsync(int ofertaId, int usuarioId, bool incluirPar = false);
    Task<(List<OfertasVenta> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, FiltroOfertasRequest filtro);
    Task<List<NivelOrdenDto>> ObtenerNivelesVentaAsync(int parMonedaId, int limite = 20);
    Task<List<LibroOrdenEntradaDto>> ObtenerEntradasVentaAsync(int parMonedaId, int limite = 10);
    Task<List<OfertasVenta>> ObtenerActivasPorParAsync(int parMonedaId, decimal? precioMaximo = null);
    Task AgregarAsync(OfertasVenta oferta);
    Task GuardarCambiosAsync();
}
