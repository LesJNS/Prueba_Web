using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using X_Chang.API.Models;

namespace X_Chang.API.Authentication;

public class SessionAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ExchangeDivisasDbContext _context;

    public SessionAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ExchangeDivisasDbContext context)
        : base(options, logger, encoder)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.NoResult();

        var sesion = await _context.SesionesUsuario
            .Include(s => s.Usuario)
            .ThenInclude(u => u.Rol)
            .FirstOrDefaultAsync(s =>
                s.TokenSesion == token &&
                s.Estado == "Activa" &&
                s.FechaExpiracion > DateTime.UtcNow);

        if (sesion == null)
            return AuthenticateResult.Fail("Token inválido o expirado.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, sesion.Usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Name, sesion.Usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, sesion.Usuario.CorreoElectronico),
            new Claim(ClaimTypes.Role, sesion.Usuario.Rol.Nombre)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
