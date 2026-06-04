using System.Security.Claims;

namespace X_Chang.CORE.Interfaces;

public interface IJwtService
{
    string GenerarToken(IEnumerable<Claim> claims);
    string GenerarRefreshToken();
    ClaimsPrincipal? ObtenerPrincipalDesdeToken(string token);
}
