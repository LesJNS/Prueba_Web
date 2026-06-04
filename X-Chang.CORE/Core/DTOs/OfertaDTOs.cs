namespace X_Chang.CORE.DTOs;

public record CrearOfertaRequest(int ParMonedaId, decimal Cantidad, decimal PrecioUnitario);
public record OfertaDto(
    int OfertaVentaId, int ParMonedaId,
    string MonedaOrigen, string MonedaDestino,
    decimal CantidadOriginal, decimal CantidadVendida, decimal CantidadPendiente,
    decimal PrecioUnitario, decimal TotalEsperado, decimal TotalRecibido,
    string Estado, DateTime FechaCreacion, DateTime FechaActualizacion);
