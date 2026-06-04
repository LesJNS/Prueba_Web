namespace X_Chang.CORE.DTOs;

public record AdminUsuarioDto(
    int UsuarioId, string NombreUsuario, string CorreoElectronico,
    string Rol, string Pais, string Estado,
    DateTime FechaRegistro, DateTime? FechaUltimoAcceso);

public record CambiarEstadoUsuarioRequest(int UsuarioId, string NuevoEstado, string Motivo);

public record RestriccionRequest(
    int UsuarioId, string TipoAccion, string Mensaje,
    DateTime FechaInicio, DateTime? FechaFin);

public record RestriccionDto(
    int RestriccionId, string NombreUsuario, string TipoAccion, string Mensaje,
    DateTime FechaInicio, DateTime? FechaFin, string EstadoRestriccion);

public record AuditoriaDto(
    int AuditoriaId, string Administrador, string UsuarioAfectado,
    string TipoAccion, string MensajeRegistrado, DateTime FechaHora);

public record DashboardDto(
    int TotalUsuarios, int UsuariosActivos,
    int OrdenesActivasCompra, int OrdenesActivasVenta,
    int OperacionesHoy, decimal VolumenHoyCompra, decimal VolumenHoyVenta,
    List<ParVolumenDto> TopPares);

public record ParVolumenDto(string Par, string MonedaOrigen, string MonedaDestino, decimal VolumenCompra, decimal VolumenVenta);

public record ConfiguracionDto(int ConfiguracionId, string Clave, string Valor, string? Descripcion, DateTime FechaActualizacion);

public record ActualizarConfiguracionRequest(string Valor, string? Descripcion);

public record FiltroAdminRequest(string? Filtro, string? Estado, int Pagina = 1, int TamanoPagina = 20);
