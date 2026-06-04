namespace X_Chang.CORE.DTOs;

public record OrdenDto(
    int OrdenCompraId, int ParMonedaId,
    string MonedaOrigen, string MonedaDestino,
    decimal CantidadOriginal, decimal CantidadObtenida, decimal CantidadPendiente,
    decimal PrecioUnitario, decimal TotalComprometido, decimal TotalEjecutado,
    string Estado, DateTime FechaCreacion, DateTime FechaActualizacion);

public record FiltroOrdenesRequest(DateTime? Desde, DateTime? Hasta, string? Estado, int Pagina = 1, int TamanoPagina = 10);
