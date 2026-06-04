namespace X_Chang.CORE.DTOs;

public record OfertaDto(
    int OfertaVentaId, int ParMonedaId,
    string MonedaOrigen, string MonedaDestino,
    decimal CantidadOriginal, decimal CantidadVendida, decimal CantidadPendiente,
    decimal PrecioUnitario, decimal TotalEsperado, decimal TotalRecibido,
    string Estado, DateTime FechaCreacion, DateTime FechaActualizacion);

public record FiltroOfertasRequest(DateTime? Desde, DateTime? Hasta, string? Estado, int Pagina = 1, int TamanoPagina = 10);
