namespace X_Chang.CORE.DTOs;

public record PagedResult<T>(List<T> Items, int Total, int Pagina, int TamanoPagina);
