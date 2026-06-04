namespace X_Chang.CORE.DTOs;

public record AdminUsuarioDto(
    int UsuarioId, string NombreUsuario, string CorreoElectronico,
    string Rol, string Pais, string Estado,
    DateTime FechaRegistro, DateTime? FechaUltimoAcceso);

public record CambiarEstadoUsuarioRequest(int UsuarioId, string NuevoEstado, string Motivo);

public record RestringirUsuarioRequest(int UsuarioId, string Mensaje);
public record HabilitarUsuarioRequest(int UsuarioId, string Mensaje);

public record RestriccionRequest(
    int UsuarioId, string TipoAccion, string Mensaje,
    DateTime FechaInicio, DateTime? FechaFin);

public record RestriccionDto(
    int RestriccionId, string NombreUsuario, string TipoAccion, string Mensaje,
    DateTime FechaInicio, DateTime? FechaFin, string EstadoRestriccion);

public record AuditoriaDto(
    int AuditoriaId, string Administrador, string UsuarioAfectado,
    string TipoAccion, string MensajeRegistrado, DateTime FechaHora);

public record FiltroAuditoriaRequest(
    DateTime? Desde, DateTime? Hasta,
    string? Administrador, string? UsuarioAfectado, string? TipoAccion,
    int Pagina = 1, int TamanoPagina = 20);

public record DashboardDto(
    int TotalUsuarios, int UsuariosActivos,
    int OrdenesActivasCompra, int OrdenesActivasVenta,
    int TransaccionesEjecutadas,
    decimal TotalDepositos, decimal TotalRetiros, decimal VolumenTotal,
    List<ParVolumenDto> TopPares,
    List<DashboardDiaDto> VolumenPorDia,
    List<DashboardDiaDto> OperacionesPorDia,
    List<DashboardMonedaDto> VolumenPorMoneda,
    List<DashboardTipoDto> DistribucionPorTipo);

public record DashboardDiaDto(DateTime Dia, decimal Valor);
public record DashboardMonedaDto(string Moneda, decimal VolumenTotal, int CantidadOperaciones);
public record DashboardTipoDto(string TipoOperacion, int Cantidad);
public record FiltroDashboardRequest(DateTime? Desde, DateTime? Hasta);

public record ParVolumenDto(string Par, string MonedaOrigen, string MonedaDestino, decimal VolumenCompra, decimal VolumenVenta);

public record ConfiguracionDto(int ConfiguracionId, string Clave, string Valor, string? Descripcion, DateTime FechaActualizacion);

public record ActualizarConfiguracionRequest(string Valor, string? Descripcion);

public record FiltroAdminRequest(
    string? NombreUsuario, string? CorreoElectronico, string? Estado,
    string? Filtro,
    int Pagina = 1, int TamanoPagina = 20);
