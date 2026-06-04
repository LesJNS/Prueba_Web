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
