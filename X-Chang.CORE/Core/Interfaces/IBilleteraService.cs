using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IBilleteraService
{
    Task<BilleteraDto> ObtenerBilleteraAsync(int usuarioId);
    Task<DepositoDto> DepositarAsync(int usuarioId, DepositoRequest request);
    Task<RetiroDto> RetirarAsync(int usuarioId, RetiroRequest request);
    Task<PagedResult<MovimientoDto>> ObtenerMovimientosAsync(int usuarioId, int pagina, int tamano);
    Task<List<DepositoDto>> ObtenerDepositosAsync(int usuarioId);
    Task<List<RetiroDto>> ObtenerRetirosAsync(int usuarioId);
}
