using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.DTOs;

public record LoginRequest(
    [Required][StringLength(100, MinimumLength = 2)] string IdentificadorAcceso,
    [Required][StringLength(50, MinimumLength = 8)] string Password);

public record RegisterRequest(
    [Required][StringLength(30, MinimumLength = 2)] string NombreUsuario,
    [Required][StringLength(100, MinimumLength = 5)][EmailAddress] string CorreoElectronico,
    [Required][StringLength(50, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,50}$",
        ErrorMessage = "La contraseña debe contener una mayúscula, un número y un carácter especial")]
    string Password,
    [Required][StringLength(50, MinimumLength = 8)] string ConfirmarPassword,
    [Required][Range(1, int.MaxValue, ErrorMessage = "Seleccione un país")] int PaisId);

public record AuthResponse(string Token, DateTime Expira, UsuarioInfoDto Usuario);
public record UsuarioInfoDto(int UsuarioId, string NombreUsuario, string CorreoElectronico, string Rol, string TemaVisual, string Estado);
public record RefreshTokenRequest(string Token);
