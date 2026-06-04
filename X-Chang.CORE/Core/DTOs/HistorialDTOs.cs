namespace X_Chang.CORE.DTOs;

public record HistorialDto(
    int HistorialId, string TipoOperacion, int ReferenciaId,
    string? ParMoneda, string? CodigoMoneda,
    DateTime FechaHora, string Estado, string? MetodoEjecucion);

public record FiltroHistorialRequest(
    DateTime? Desde, DateTime? Hasta,
    string? TipoOperacion, string? Estado,
    int Pagina = 1, int TamanoPagina = 20);

public record PagedResult<T>(List<T> Items, int Total, int Pagina, int TamanoPagina);

public record FiltroColumnaRequest(DateTime? Desde, DateTime? Hasta, int Pagina = 1, int TamanoPagina = 10);

public record HistorialOrdenDto(
    int OrdenCompraId, DateTime FechaHora, string Par,
    decimal CantidadOriginal, decimal CantidadObtenida, decimal CantidadPendiente,
    decimal PrecioUnitario, decimal TotalComprometido, decimal TotalEjecutado, string Estado);

public record HistorialOfertaDto(
    int OfertaVentaId, DateTime FechaHora, string Par,
    decimal CantidadOriginal, decimal CantidadVendida, decimal CantidadPendiente,
    decimal PrecioUnitario, decimal TotalEsperado, decimal TotalRecibido, string Estado);

public record HistorialCompraInmediataDto(
    int OperacionId, DateTime FechaHora, string Par,
    decimal CantidadObtenida, decimal? PrecioMinimo, decimal? PrecioMaximo, decimal? PrecioPromedio,
    decimal? TotalPagado, string Estado, string MetodoEjecucion,
    bool TieneSubOperaciones, List<HistorialCompraInmediataDto>? SubOperaciones);

public record HistorialVentaInmediataDto(
    int OperacionId, DateTime FechaHora, string Par,
    decimal CantidadVendida, decimal? PrecioMinimo, decimal? PrecioMaximo, decimal? PrecioPromedio,
    decimal? TotalRecibido, string Estado, string MetodoEjecucion,
    bool TieneSubOperaciones, List<HistorialVentaInmediataDto>? SubOperaciones);

public record HistorialDepositoDto(
    int DepositoId, DateTime FechaHora, string Moneda,
    decimal MontoDepositado, string MetodoPago,
    decimal ComisionAplicada, decimal TotalPagado, string Estado);

public record HistorialRetiroDto(
    int RetiroId, DateTime FechaHora, string Moneda,
    decimal MontoRetirado, string MetodoCobro,
    decimal ComisionAplicada, decimal MontoFinalRecibido, string Estado);

public record HistorialCompletoDto(
    PagedResult<HistorialOrdenDto> Ordenes,
    PagedResult<HistorialOfertaDto> Ofertas,
    PagedResult<HistorialCompraInmediataDto> ComprasInmediatas,
    PagedResult<HistorialVentaInmediataDto> VentasInmediatas,
    PagedResult<HistorialDepositoDto> Depositos,
    PagedResult<HistorialRetiroDto> Retiros);
