namespace X_Chang.CORE.DTOs;

public record BuscarRutaRequest(int ParMonedaId, string TipoOperacion, decimal Cantidad, int MaxSaltos = 3);

public record RutaConversionDto(
    int RutaConversionId,
    string MonedaInicial, string MonedaFinal,
    int CantidadSaltos, decimal TotalEstimado,
    decimal? AhorroEstimado, decimal? GananciaEstimada,
    List<SaltoRutaDto> Saltos);

public record SaltoRutaDto(
    int NumeroSalto,
    string CodigoOrigen, string CodigoDestino,
    string MonedaOrigen, string MonedaDestino,
    decimal CantidadConvertida, decimal ResultadoObtenido, decimal? PrecioPromedio);

public record BusquedaRutaDto(
    int BusquedaRutaId,
    string MonedaOrigen, string MonedaDestino,
    string TipoOperacion, decimal CantidadSolicitada, int MaxSaltos,
    string Estado, decimal? AhorroEstimado, decimal? GananciaEstimada,
    DateTime FechaInicio, List<RutaConversionDto>? Rutas);
