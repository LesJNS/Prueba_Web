namespace X_Chang.CORE.DTOs;

public record ParMonedasDto(
    int ParMonedaId, string MonedaOrigen, string MonedaDestino, string Par,
    decimal? MayorPrecioCompra, decimal? MenorPrecioVenta, decimal? Margen,
    decimal VolumenTotal, DateTime? UltimaTransaccion);

public record ParMonedasDetalleDto(
    int ParMonedaId, string MonedaOrigen, string MonedaDestino, string Par,
    decimal? MayorPrecioCompra, decimal? MenorPrecioVenta, decimal? Margen,
    List<HistoricoParDto> Historico);

public record HistoricoParDto(
    DateTime FechaRegistro,
    decimal? MayorPrecioCompra,
    decimal? MenorPrecioVenta,
    decimal? Margen,
    decimal VolumenCompra,
    decimal VolumenVenta);

public record FiltroParesRequest(
    string? MonedaOrigen, string? MonedaDestino,
    string OrdenarPor = "FechaReciente",
    string Direccion = "Desc",
    bool ColapsarInversos = false,
    int Pagina = 1, int TamanoPagina = 20);

public record FiltroHistoricoRequest(string RangoTemporal = "UltimoDia");

public record GraficoPrincipalDto(
    ParMonedasDetalleDto PrimerGrafico, ParMonedasDetalleDto? SegundoGrafico);

public record MetodoPagoDto(
    int MetodoPagoId, string Nombre, string Tipo,
    decimal ComisionPorcentaje, decimal ComisionFija);
