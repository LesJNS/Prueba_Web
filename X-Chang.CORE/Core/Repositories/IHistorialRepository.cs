using X_Chang.API.Models;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Repositories;

public interface IHistorialRepository
{
    Task<(List<HistorialTransacciones> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, FiltroHistorialRequest filtro);
    Task<(List<OrdenesCompra> Items, int Total)> ObtenerOrdenesAsync(int usuarioId, FiltroColumnaRequest filtro);
    Task<(List<OfertasVenta> Items, int Total)> ObtenerOfertasAsync(int usuarioId, FiltroColumnaRequest filtro);
    Task<(List<OperacionesInmediatas> Items, int Total)> ObtenerComprasInmediatasAsync(int usuarioId, FiltroColumnaRequest filtro);
    Task<(List<OperacionesInmediatas> Items, int Total)> ObtenerVentasInmediatasAsync(int usuarioId, FiltroColumnaRequest filtro);
    Task<(List<Depositos> Items, int Total)> ObtenerDepositosAsync(int usuarioId, FiltroColumnaRequest filtro);
    Task<(List<Retiros> Items, int Total)> ObtenerRetirosAsync(int usuarioId, FiltroColumnaRequest filtro);
    Task AgregarAsync(HistorialTransacciones historial);
    Task GuardarCambiosAsync();
}
