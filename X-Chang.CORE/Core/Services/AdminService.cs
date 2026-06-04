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

        if (!string.IsNullOrWhiteSpace(filtro.NombreUsuario))
            query = query.Where(u => u.NombreUsuario.Contains(filtro.NombreUsuario));

        if (!string.IsNullOrWhiteSpace(filtro.CorreoElectronico))
            query = query.Where(u => u.CorreoElectronico.Contains(filtro.CorreoElectronico));

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
        var estadosValidos = new[] { "Activo", "Bloqueado", "Suspendido", "Restringido" };
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

    public async Task RestringirUsuarioAsync(int adminId, RestringirUsuarioRequest request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.OrdenesCompra.Where(o => o.Estado == "Activa" || o.Estado == "Parcial"))
            .ThenInclude(o => o.ParMoneda)
            .Include(u => u.OfertasVenta.Where(o => o.Estado == "Activa" || o.Estado == "Parcial"))
            .ThenInclude(o => o.ParMoneda)
            .FirstOrDefaultAsync(u => u.UsuarioId == request.UsuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (usuario.Estado == "Restringido")
            throw new InvalidOperationException("El usuario ya está restringido.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == request.UsuarioId);

            foreach (var orden in usuario.OrdenesCompra)
            {
                var montoReembolso = orden.CantidadPendiente * orden.PrecioUnitario;
                orden.Estado = "Cancelada";
                orden.FechaCancelacion = DateTime.UtcNow;
                orden.FechaActualizacion = DateTime.UtcNow;

                if (billetera != null)
                {
                    var saldo = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
                        _context, billetera.BilleteraId, orden.ParMoneda.MonedaOrigenId);
                    var ant = saldo.SaldoDisponible;
                    saldo.SaldoDisponible += montoReembolso;
                    saldo.FechaActualizacion = DateTime.UtcNow;
                    _context.MovimientosBilletera.Add(new MovimientosBilletera
                    {
                        UsuarioId = request.UsuarioId, MonedaId = orden.ParMoneda.MonedaOrigenId,
                        TipoMovimiento = "DevolucionOrden", Monto = montoReembolso,
                        SaldoAnterior = ant, SaldoPosterior = saldo.SaldoDisponible,
                        FechaMovimiento = DateTime.UtcNow,
                        ReferenciaTipo = "OrdenCompra", ReferenciaId = orden.OrdenCompraId
                    });
                }
            }

            foreach (var oferta in usuario.OfertasVenta)
            {
                oferta.Estado = "Cancelada";
                oferta.FechaCancelacion = DateTime.UtcNow;
                oferta.FechaActualizacion = DateTime.UtcNow;

                if (billetera != null)
                {
                    var saldo = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
                        _context, billetera.BilleteraId, oferta.ParMoneda.MonedaDestinoId);
                    var ant = saldo.SaldoDisponible;
                    saldo.SaldoDisponible += oferta.CantidadPendiente;
                    saldo.FechaActualizacion = DateTime.UtcNow;
                    _context.MovimientosBilletera.Add(new MovimientosBilletera
                    {
                        UsuarioId = request.UsuarioId, MonedaId = oferta.ParMoneda.MonedaDestinoId,
                        TipoMovimiento = "DevolucionOferta", Monto = oferta.CantidadPendiente,
                        SaldoAnterior = ant, SaldoPosterior = saldo.SaldoDisponible,
                        FechaMovimiento = DateTime.UtcNow,
                        ReferenciaTipo = "OfertaVenta", ReferenciaId = oferta.OfertaVentaId
                    });
                }
            }

            usuario.Estado = "Restringido";

            _context.AuditoriaAdministrativa.Add(new AuditoriaAdministrativa
            {
                AdministradorId = adminId,
                UsuarioAfectadoId = request.UsuarioId,
                TipoAccion = "Restriccion",
                MensajeRegistrado = request.Mensaje,
                FechaHora = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task HabilitarUsuarioAsync(int adminId, HabilitarUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(request.UsuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (usuario.Estado == "Activo")
            throw new InvalidOperationException("El usuario ya está activo.");

        usuario.Estado = "Activo";

        _context.AuditoriaAdministrativa.Add(new AuditoriaAdministrativa
        {
            AdministradorId = adminId,
            UsuarioAfectadoId = request.UsuarioId,
            TipoAccion = "Habilitacion",
            MensajeRegistrado = request.Mensaje,
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

    public async Task<PagedResult<AuditoriaDto>> ObtenerAuditoriaAsync(FiltroAuditoriaRequest filtro)
    {
        var query = _context.AuditoriaAdministrativa
            .Include(a => a.Administrador)
            .Include(a => a.UsuarioAfectado)
            .AsQueryable();

        if (filtro.Desde.HasValue) query = query.Where(a => a.FechaHora >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) query = query.Where(a => a.FechaHora <= filtro.Hasta.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Administrador))
            query = query.Where(a => a.Administrador.NombreUsuario.Contains(filtro.Administrador));
        if (!string.IsNullOrWhiteSpace(filtro.UsuarioAfectado))
            query = query.Where(a => a.UsuarioAfectado.NombreUsuario.Contains(filtro.UsuarioAfectado));
        if (!string.IsNullOrWhiteSpace(filtro.TipoAccion) && filtro.TipoAccion != "Todos")
            query = query.Where(a => a.TipoAccion == filtro.TipoAccion);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.FechaHora)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(a => new AuditoriaDto(
                a.AuditoriaId,
                a.Administrador.NombreUsuario,
                a.UsuarioAfectado.NombreUsuario,
                a.TipoAccion,
                a.MensajeRegistrado,
                a.FechaHora))
            .ToListAsync();

        return new PagedResult<AuditoriaDto>(items, total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<DashboardDto> ObtenerDashboardAsync(FiltroDashboardRequest filtro)
    {
        var desde = filtro.Desde ?? DateTime.UtcNow.Date.AddDays(-30);
        var hasta = filtro.Hasta ?? DateTime.UtcNow;

        var totalUsuarios = await _context.Usuarios.CountAsync();
        var usuariosActivos = await _context.Usuarios
            .CountAsync(u => u.FechaUltimoAcceso >= desde && u.FechaUltimoAcceso <= hasta);
        var ordenesActivasCompra = await _context.OrdenesCompra.CountAsync(o => o.Estado == "Activa");
        var ordenesActivasVenta = await _context.OfertasVenta.CountAsync(o => o.Estado == "Activa");
        var transacciones = await _context.EjecucionesOrden
            .CountAsync(e => e.FechaEjecucion >= desde && e.FechaEjecucion <= hasta);

        var totalDepositos = await _context.Depositos
            .Where(d => d.FechaDeposito >= desde && d.FechaDeposito <= hasta)
            .SumAsync(d => (decimal?)d.MontoDepositado) ?? 0;

        var totalRetiros = await _context.Retiros
            .Where(r => r.FechaRetiro >= desde && r.FechaRetiro <= hasta)
            .SumAsync(r => (decimal?)r.MontoRetirado) ?? 0;

        var volumenTotal = await _context.EjecucionesOrden
            .Where(e => e.FechaEjecucion >= desde && e.FechaEjecucion <= hasta)
            .SumAsync(e => (decimal?)e.CantidadEjecutada) ?? 0;

        var topPares = await _context.EjecucionesOrden
            .Include(e => e.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(e => e.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(e => e.FechaEjecucion >= desde && e.FechaEjecucion <= hasta)
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

        var volumenPorDia = await _context.EjecucionesOrden
            .Where(e => e.FechaEjecucion >= desde && e.FechaEjecucion <= hasta)
            .GroupBy(e => e.FechaEjecucion.Date)
            .Select(g => new DashboardDiaDto(g.Key, g.Sum(e => e.CantidadEjecutada)))
            .OrderBy(d => d.Dia)
            .ToListAsync();

        var operacionesPorDia = await _context.EjecucionesOrden
            .Where(e => e.FechaEjecucion >= desde && e.FechaEjecucion <= hasta)
            .GroupBy(e => e.FechaEjecucion.Date)
            .Select(g => new DashboardDiaDto(g.Key, g.Count()))
            .OrderBy(d => d.Dia)
            .ToListAsync();

        var volumenPorMoneda = await _context.EjecucionesOrden
            .Include(e => e.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Where(e => e.FechaEjecucion >= desde && e.FechaEjecucion <= hasta)
            .GroupBy(e => e.ParMoneda.MonedaOrigen.CodigoIso)
            .Select(g => new DashboardMonedaDto(g.Key, g.Sum(e => e.CantidadEjecutada), g.Count()))
            .OrderByDescending(m => m.VolumenTotal)
            .ToListAsync();

        var distribucionPorTipo = await _context.HistorialTransacciones
            .Where(h => h.FechaHora >= desde && h.FechaHora <= hasta)
            .GroupBy(h => h.TipoOperacion)
            .Select(g => new DashboardTipoDto(g.Key, g.Count()))
            .ToListAsync();

        return new DashboardDto(
            totalUsuarios, usuariosActivos,
            ordenesActivasCompra, ordenesActivasVenta,
            transacciones, totalDepositos, totalRetiros, volumenTotal,
            topPares, volumenPorDia, operacionesPorDia,
            volumenPorMoneda, distribucionPorTipo);
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
