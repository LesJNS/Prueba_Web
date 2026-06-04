namespace X_Chang.CORE.DTOs;

public record OperacionInmediataRequest(
    int ParMonedaId,
    string TipoOperacion,
    decimal Cantidad,
    decimal? PrecioMinimo,
    decimal? PrecioMaximo);

public record OperacionInmediataDto(
    int OperacionInmediataId, int ParMonedaId,
    string MonedaOrigen, string MonedaDestino,
    string TipoOperacion, string MetodoEjecucion,
    decimal CantidadSolicitada, decimal CantidadEjecutada,
    decimal? PrecioMinimo, decimal? PrecioMaximo, decimal? PrecioPromedio,
    decimal? TotalPagado, decimal? TotalRecibido,
    string Estado, DateTime FechaOperacion,
    List<EjecucionDto>? Ejecuciones);

public record EjecucionDto(
    int EjecucionId, int ParMonedaId,
    string MonedaOrigen, string MonedaDestino,
    decimal CantidadEjecutada, decimal PrecioUnitario, decimal TotalOperacion,
    DateTime FechaEjecucion);
