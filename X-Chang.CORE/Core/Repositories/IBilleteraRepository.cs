using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public interface IBilleteraRepository
{
    Task<Billeteras?> ObtenerPorUsuarioAsync(int usuarioId);
    Task<Billeteras?> ObtenerConSaldosAsync(int usuarioId);
    Task<SaldosBilletera?> ObtenerSaldoAsync(int billeteraId, int monedaId);
    Task<SaldosBilletera> ObtenerOCrearSaldoAsync(int billeteraId, int monedaId);
    Task<MetodosPago?> ObtenerMetodoPagoPorIdAsync(int metodoPagoId);
    Task<Monedas?> ObtenerMonedaPorIdAsync(int monedaId);
    Task<List<MetodosPagoPais>> ObtenerMetodosPorPaisAsync(int paisId);
    Task<(List<MovimientosBilletera> Items, int Total)> ObtenerMovimientosAsync(int usuarioId, int pagina, int tamano);
    Task<List<Depositos>> ObtenerDepositosAsync(int usuarioId);
    Task<List<Retiros>> ObtenerRetirosAsync(int usuarioId);
    Task AgregarDepositoAsync(Depositos deposito);
    Task AgregarRetiroAsync(Retiros retiro);
    Task AgregarMovimientoAsync(MovimientosBilletera movimiento);
    Task GuardarCambiosAsync();
}
