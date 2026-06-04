using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegistrarAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(int usuarioId, string refreshToken);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
