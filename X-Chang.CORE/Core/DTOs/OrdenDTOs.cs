namespace X_Chang.CORE.DTOs;

public record CrearOrdenRequest(int ParMonedaId, decimal Cantidad, decimal PrecioUnitario);
public record OrdenDto(
    int OrdenCompraId, int ParMonedaId,
    string MonedaOrigen, string MonedaDestino,
    decimal CantidadOriginal, decimal CantidadObtenida, decimal CantidadPendiente,
    decimal PrecioUnitario, decimal TotalComprometido, decimal TotalEjecutado,
    string Estado, DateTime FechaCreacion, DateTime FechaActualizacion);
public record LibroOrdenesDto(List<NivelOrdenDto> Compras, List<NivelOrdenDto> Ventas);
public record NivelOrdenDto(decimal Precio, decimal CantidadTotal, int NumeroOrdenes);
