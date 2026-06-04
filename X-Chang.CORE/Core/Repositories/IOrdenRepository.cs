using X_Chang.API.Models;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Repositories;

public interface IOrdenRepository
{
    Task<OrdenesCompra?> ObtenerPorIdAsync(int ordenId, bool incluirPar = false);
    Task<OrdenesCompra?> ObtenerPorIdYUsuarioAsync(int ordenId, int usuarioId, bool incluirPar = false);
    Task<(List<OrdenesCompra> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, FiltroOrdenesRequest filtro);
    Task<List<NivelOrdenDto>> ObtenerNivelesCompraAsync(int parMonedaId, int limite = 20);
    Task<List<LibroOrdenEntradaDto>> ObtenerEntradasCompraAsync(int parMonedaId, int limite = 10);
    Task<List<OrdenesCompra>> ObtenerActivasPorParAsync(int parMonedaId, decimal? precioMinimo = null);
    Task AgregarAsync(OrdenesCompra orden);
    Task GuardarCambiosAsync();
}
