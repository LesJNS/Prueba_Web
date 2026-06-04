namespace X_Chang.CORE.DTOs;

public record LoginRequest(string CorreoElectronico, string Password);
public record RegisterRequest(string NombreUsuario, string CorreoElectronico, string Password, int PaisId);
public record AuthResponse(string Token, string RefreshToken, DateTime Expira, UsuarioInfoDto Usuario);
public record UsuarioInfoDto(int UsuarioId, string NombreUsuario, string CorreoElectronico, string Rol, string TemaVisual, string Estado);
public record RefreshTokenRequest(string Token, string RefreshToken);
public record CambiarPasswordRequest(string PasswordActual, string PasswordNuevo);
