using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class AdminService : IAdminService
{
    private readonly ExchangeDivisasDbContext _context;

    public AdminService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminUsuarioDto>> ObtenerUsuariosAsync(FiltroAdminRequest filtro)
    {
        var query = _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Pais)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Filtro))
        {
            var f = filtro.Filtro.ToLower();
            query = query.Where(u =>
                u.NombreUsuario.ToLower().Contains(f) ||
                u.CorreoElectronico.ToLower().Contains(f));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(u => u.Estado == filtro.Estado);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.FechaRegistro)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(u => new AdminUsuarioDto(
                u.UsuarioId, u.NombreUsuario, u.CorreoElectronico,
                u.Rol.Nombre, u.Pais.Nombre, u.Estado,
                u.FechaRegistro, u.FechaUltimoAcceso))
            .ToListAsync();

        return new PagedResult<AdminUsuarioDto>(items, total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<AdminUsuarioDto> ObtenerUsuarioAsync(int usuarioId)
    {
        var u = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Pais)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        return new AdminUsuarioDto(
            u.UsuarioId, u.NombreUsuario, u.CorreoElectronico,
            u.Rol.Nombre, u.Pais.Nombre, u.Estado,
            u.FechaRegistro, u.FechaUltimoAcceso);
    }

    public async Task CambiarEstadoUsuarioAsync(int adminId, CambiarEstadoUsuarioRequest request)
    {
        var estadosValidos = new[] { "Activo", "Bloqueado", "Suspendido" };
        if (!estadosValidos.Contains(request.NuevoEstado))
            throw new ArgumentException($"Estado no válido. Permitidos: {string.Join(", ", estadosValidos)}");

        var usuario = await _context.Usuarios.FindAsync(request.UsuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        var estadoAnterior = usuario.Estado;
        usuario.Estado = request.NuevoEstado;

        _context.AuditoriaAdministrativa.Add(new AuditoriaAdministrativa
        {
            AdministradorId = adminId,
            UsuarioAfectadoId = request.UsuarioId,
            TipoAccion = "CambioEstado",
            MensajeRegistrado = $"Estado cambiado de {estadoAnterior} a {request.NuevoEstado}. Motivo: {request.Motivo}",
            FechaHora = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<RestriccionDto> AplicarRestriccionAsync(int adminId, RestriccionRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(request.UsuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        var restriccion = new RestriccionesUsuario
        {
            UsuarioId = request.UsuarioId,
            AdministradorId = adminId,
            TipoAccion = request.TipoAccion,
            Mensaje = request.Mensaje,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            EstadoRestriccion = "Activa"
        };
        _context.RestriccionesUsuario.Add(restriccion);

        _context.AuditoriaAdministrativa.Add(new AuditoriaAdministrativa
        {
            AdministradorId = adminId,
            UsuarioAfectadoId = request.UsuarioId,
            TipoAccion = "AplicarRestriccion",
            MensajeRegistrado = $"Restricción '{request.TipoAccion}' aplicada. Motivo: {request.Mensaje}",
            FechaHora = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new RestriccionDto(
            restriccion.RestriccionId,
            usuario.NombreUsuario,
            restriccion.TipoAccion,
            restriccion.Mensaje,
            restriccion.FechaInicio,
            restriccion.FechaFin,
            restriccion.EstadoRestriccion);
    }

    public async Task<PagedResult<AuditoriaDto>> ObtenerAuditoriaAsync(
        DateTime? desde, DateTime? hasta, int pagina, int tamano)
    {
        var query = _context.AuditoriaAdministrativa
            .Include(a => a.Administrador)
            .Include(a => a.UsuarioAfectado)
            .AsQueryable();

        if (desde.HasValue) query = query.Where(a => a.FechaHora >= desde.Value);
        if (hasta.HasValue) query = query.Where(a => a.FechaHora <= hasta.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.FechaHora)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(a => new AuditoriaDto(
                a.AuditoriaId,
                a.Administrador.NombreUsuario,
                a.UsuarioAfectado.NombreUsuario,
                a.TipoAccion,
                a.MensajeRegistrado,
                a.FechaHora))
            .ToListAsync();

        return new PagedResult<AuditoriaDto>(items, total, pagina, tamano);
    }

    public async Task<DashboardDto> ObtenerDashboardAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var manana = hoy.AddDays(1);

        var totalUsuarios = await _context.Usuarios.CountAsync();
        var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Estado == "Activo");
        var ordenesActivasCompra = await _context.OrdenesCompra.CountAsync(o => o.Estado == "Activa");
        var ordenesActivasVenta = await _context.OfertasVenta.CountAsync(o => o.Estado == "Activa");
        var operacionesHoy = await _context.HistorialTransacciones
            .CountAsync(h => h.FechaHora >= hoy && h.FechaHora < manana);

        var volumenHoy = await _context.EjecucionesOrden
            .Where(e => e.FechaEjecucion >= hoy && e.FechaEjecucion < manana)
            .GroupBy(e => 1)
            .Select(g => new
            {
                VolumenCompra = g.Sum(e => e.CantidadEjecutada),
                VolumenVenta = g.Sum(e => e.TotalOperacion)
            })
            .FirstOrDefaultAsync();

        var topPares = await _context.EjecucionesOrden
            .Include(e => e.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(e => e.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .Where(e => e.FechaEjecucion >= hoy && e.FechaEjecucion < manana)
            .GroupBy(e => new
            {
                e.ParMonedaId,
                OrigenCodigo = e.ParMoneda.MonedaOrigen.CodigoIso,
                DestinoCodigo = e.ParMoneda.MonedaDestino.CodigoIso
            })
            .Select(g => new ParVolumenDto(
                g.Key.OrigenCodigo + "/" + g.Key.DestinoCodigo,
                g.Key.OrigenCodigo,
                g.Key.DestinoCodigo,
                g.Sum(e => e.CantidadEjecutada),
                g.Sum(e => e.TotalOperacion)))
            .OrderByDescending(p => p.VolumenCompra + p.VolumenVenta)
            .Take(5)
            .ToListAsync();

        return new DashboardDto(
            totalUsuarios, usuariosActivos,
            ordenesActivasCompra, ordenesActivasVenta,
            operacionesHoy,
            volumenHoy?.VolumenCompra ?? 0,
            volumenHoy?.VolumenVenta ?? 0,
            topPares);
    }

    public async Task<List<ConfiguracionDto>> ObtenerConfiguracionesAsync()
    {
        return await _context.ConfiguracionSistema
            .OrderBy(c => c.Clave)
            .Select(c => new ConfiguracionDto(
                c.ConfiguracionId, c.Clave, c.Valor, c.Descripcion, c.FechaActualizacion))
            .ToListAsync();
    }

    public async Task<ConfiguracionDto> ActualizarConfiguracionAsync(
        int adminId, int configuracionId, ActualizarConfiguracionRequest request)
    {
        var config = await _context.ConfiguracionSistema.FindAsync(configuracionId)
            ?? throw new InvalidOperationException("Configuración no encontrada.");

        var valorAnterior = config.Valor;
        config.Valor = request.Valor;
        if (request.Descripcion != null) config.Descripcion = request.Descripcion;
        config.FechaActualizacion = DateTime.UtcNow;

        _context.AuditoriaAdministrativa.Add(new AuditoriaAdministrativa
        {
            AdministradorId = adminId,
            UsuarioAfectadoId = adminId,
            TipoAccion = "CambioConfiguracion",
            MensajeRegistrado = $"Config '{config.Clave}': '{valorAnterior}' → '{request.Valor}'",
            FechaHora = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new ConfiguracionDto(
            config.ConfiguracionId, config.Clave, config.Valor,
            config.Descripcion, config.FechaActualizacion);
    }
}
