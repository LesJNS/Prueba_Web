namespace X_Chang.CORE.DTOs;

public record BilleteraDto(int BilleteraId, int UsuarioId, DateTime FechaCreacion, List<SaldoDto> Saldos);
public record SaldoDto(int SaldoId, string CodigoISO, string NombreMoneda, string TipoMoneda, decimal SaldoDisponible, DateTime FechaActualizacion);
public record DepositoRequest(int MonedaId, int MetodoPagoId, decimal Monto, string? VoucherUrl);
public record DepositoDto(int DepositoId, int MonedaId, string Moneda, string MetodoPago, decimal MontoDepositado, decimal ComisionAplicada, decimal TotalPagado, string Estado, string? VoucherUrl, DateTime FechaDeposito);
public record RetiroRequest(int MonedaId, int MetodoPagoId, decimal Monto, string? VoucherUrl);
public record RetiroDto(int RetiroId, int MonedaId, string Moneda, string MetodoPago, decimal MontoRetirado, decimal ComisionAplicada, decimal MontoFinalRecibido, string Estado, string? VoucherUrl, DateTime FechaRetiro);
public record MovimientoDto(int MovimientoId, string CodigoISO, string Moneda, string TipoMovimiento, decimal Monto, decimal SaldoAnterior, decimal SaldoPosterior, DateTime FechaMovimiento, string? ReferenciaTipo, int? ReferenciaId);
