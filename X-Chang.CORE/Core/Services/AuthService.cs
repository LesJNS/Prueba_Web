using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;
using X_Chang.CORE.Settings;

namespace X_Chang.CORE.Services;

public class AuthService : IAuthService
{
    private readonly ExchangeDivisasDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        ExchangeDivisasDbContext context,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse> RegistrarAsync(RegisterRequest request)
    {
        if (await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == request.CorreoElectronico))
            throw new InvalidOperationException("El correo ya está registrado.");

        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario))
            throw new InvalidOperationException("El nombre de usuario ya está en uso.");

        var rolUsuario = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Usuario")
            ?? throw new InvalidOperationException("Rol 'Usuario' no configurado en el sistema.");

        _ = await _context.Paises.FindAsync(request.PaisId)
            ?? throw new InvalidOperationException("País no encontrado.");

        var usuario = new Usuarios
        {
            NombreUsuario = request.NombreUsuario,
            CorreoElectronico = request.CorreoElectronico,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RolId = rolUsuario.RolId,
            PaisId = request.PaisId,
            TemaVisual = "Claro",
            Estado = "Activo",
            FechaRegistro = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _context.Billeteras.Add(new Billeteras
        {
            UsuarioId = usuario.UsuarioId,
            FechaCreacion = DateTime.UtcNow
        });

        _context.AccesosUsuario.Add(new AccesosUsuario
        {
            UsuarioId = usuario.UsuarioId,
            FechaAcceso = DateTime.UtcNow,
            Exitoso = true,
            MetodoIngreso = "Registro",
            MensajeResultado = "Registro exitoso"
        });

        await _context.SaveChangesAsync();

        return await GenerarAuthResponseAsync(usuario, rolUsuario.Nombre);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.CorreoElectronico == request.CorreoElectronico);

        var exitoso = usuario != null && BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);

        if (usuario != null)
        {
            _context.AccesosUsuario.Add(new AccesosUsuario
            {
                UsuarioId = usuario.UsuarioId,
                FechaAcceso = DateTime.UtcNow,
                Exitoso = exitoso,
                MetodoIngreso = "Login",
                MensajeResultado = exitoso ? "Login exitoso" : "Credenciales inválidas"
            });
            await _context.SaveChangesAsync();
        }

        if (!exitoso || usuario == null)
            throw new UnauthorizedAccessException("Correo o contraseña incorrectos.");

        if (usuario.Estado != "Activo")
            throw new UnauthorizedAccessException($"La cuenta está {usuario.Estado.ToLower()}.");

        usuario.FechaUltimoAcceso = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerarAuthResponseAsync(usuario, usuario.Rol.Nombre);
    }

    public async Task LogoutAsync(int usuarioId, string refreshToken)
    {
        var sesion = await _context.SesionesUsuario
            .FirstOrDefaultAsync(s =>
                s.UsuarioId == usuarioId &&
                s.TokenSesion == refreshToken &&
                s.Estado == "Activa");

        if (sesion != null)
        {
            sesion.Estado = "Cerrada";
            sesion.FechaCierre = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var sesion = await _context.SesionesUsuario
            .Include(s => s.Usuario)
            .ThenInclude(u => u.Rol)
            .FirstOrDefaultAsync(s =>
                s.TokenSesion == request.RefreshToken &&
                s.Estado == "Activa");

        if (sesion == null || sesion.FechaExpiracion < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        if (sesion.Usuario.Estado != "Activo")
            throw new UnauthorizedAccessException("La cuenta no está activa.");

        sesion.Estado = "Cerrada";
        sesion.FechaCierre = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerarAuthResponseAsync(sesion.Usuario, sesion.Usuario.Rol.Nombre);
    }

    public async Task CambiarPasswordAsync(int usuarioId, CambiarPasswordRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNuevo);
        await _context.SaveChangesAsync();
    }

    private async Task<AuthResponse> GenerarAuthResponseAsync(Usuarios usuario, string rolNombre)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
            new Claim(ClaimTypes.Role, rolNombre)
        };

        var token = _jwtService.GenerarToken(claims);
        var refreshToken = _jwtService.GenerarRefreshToken();
        var expira = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes);
        var refreshExpira = DateTime.UtcNow.AddDays(_jwtSettings.RefreshExpiresInDays);

        var sesionesActivas = await _context.SesionesUsuario
            .Where(s => s.UsuarioId == usuario.UsuarioId && s.Estado == "Activa")
            .ToListAsync();

        foreach (var s in sesionesActivas)
        {
            s.Estado = "Cerrada";
            s.FechaCierre = DateTime.UtcNow;
        }

        _context.SesionesUsuario.Add(new SesionesUsuario
        {
            UsuarioId = usuario.UsuarioId,
            TokenSesion = refreshToken,
            FechaInicio = DateTime.UtcNow,
            FechaExpiracion = refreshExpira,
            Estado = "Activa"
        });

        await _context.SaveChangesAsync();

        return new AuthResponse(
            token, refreshToken, expira,
            new UsuarioInfoDto(
                usuario.UsuarioId,
                usuario.NombreUsuario,
                usuario.CorreoElectronico,
                rolNombre,
                usuario.TemaVisual,
                usuario.Estado));
    }
}
