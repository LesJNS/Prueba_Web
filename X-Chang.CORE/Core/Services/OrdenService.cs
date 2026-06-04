using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class OrdenService : IOrdenService
{
    private readonly ExchangeDivisasDbContext _context;
    private readonly IMatchingService _matching;

    public OrdenService(ExchangeDivisasDbContext context, IMatchingService matching)
    {
        _context = context;
        _matching = matching;
    }

    public async Task<OrdenDto> CrearOrdenCompraAsync(int usuarioId, CrearOrdenRequest request)
    {
        if (request.Cantidad <= 0 || request.PrecioUnitario <= 0)
            throw new ArgumentException("Cantidad y precio deben ser mayores a cero.");

        var par = await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p => p.ParMonedaId == request.ParMonedaId)
            ?? throw new InvalidOperationException("Par de moneda no encontrado.");

        if (!par.Activo)
            throw new InvalidOperationException("El par de moneda está inactivo.");

        var totalComprometido = request.Cantidad * request.PrecioUnitario;

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldoOrigen = await _context.SaldosBilletera
            .FirstOrDefaultAsync(s =>
                s.BilleteraId == billetera.BilleteraId &&
                s.MonedaId == par.MonedaOrigenId);

        if (saldoOrigen == null || saldoOrigen.SaldoDisponible < totalComprometido)
            throw new InvalidOperationException("Saldo insuficiente en la moneda de origen.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var saldoAnterior = saldoOrigen.SaldoDisponible;
            saldoOrigen.SaldoDisponible -= totalComprometido;
            saldoOrigen.FechaActualizacion = DateTime.UtcNow;

            var orden = new OrdenesCompra
            {
                UsuarioId = usuarioId,
                ParMonedaId = request.ParMonedaId,
                CantidadOriginal = request.Cantidad,
                CantidadObtenida = 0,
                CantidadPendiente = request.Cantidad,
                PrecioUnitario = request.PrecioUnitario,
                TotalComprometido = totalComprometido,
                TotalEjecutado = 0,
                Estado = "Activa",
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };
            _context.OrdenesCompra.Add(orden);
            await _context.SaveChangesAsync();

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaOrigenId,
                TipoMovimiento = "ReservaOrden",
                Monto = -totalComprometido,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldoOrigen.SaldoDisponible,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "OrdenCompra",
                ReferenciaId = orden.OrdenCompraId
            });

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "OrdenCompra",
                ReferenciaId = orden.OrdenCompraId,
                ParMonedaId = request.ParMonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Activa"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            await _matching.EjecutarMatchingOrdenAsync(orden.OrdenCompraId);

            await _context.Entry(orden).ReloadAsync();

            return MapOrdenDto(orden, par);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<PagedResult<OrdenDto>> ObtenerMisOrdenesAsync(int usuarioId, FiltroOrdenesRequest filtro)
    {
        var query = _context.OrdenesCompra
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId)
            .AsQueryable();

        if (filtro.Desde.HasValue)
            query = query.Where(o => o.FechaCreacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue)
            query = query.Where(o => o.FechaCreacion <= filtro.Hasta.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(o => o.Estado == filtro.Estado);

        query = query.OrderByDescending(o => o.FechaCreacion);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync();

        return new PagedResult<OrdenDto>(
            items.Select(o => MapOrdenDto(o, o.ParMoneda)).ToList(),
            total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<OrdenDto> ObtenerOrdenAsync(int usuarioId, int ordenId)
    {
        var orden = await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenId && o.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Orden no encontrada.");

        return MapOrdenDto(orden, orden.ParMoneda);
    }

    public async Task CancelarOrdenAsync(int usuarioId, int ordenId)
    {
        var orden = await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
            .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenId && o.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Orden no encontrada.");

        if (orden.Estado is "Cancelada" or "Completada")
            throw new InvalidOperationException($"La orden está {orden.Estado.ToLower()} y no puede cancelarse.");

        var montoReembolso = orden.CantidadPendiente * orden.PrecioUnitario;

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldoOrigen = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
            _context, billetera.BilleteraId, orden.ParMoneda.MonedaOrigenId);
        var saldoAnterior = saldoOrigen.SaldoDisponible;
        saldoOrigen.SaldoDisponible += montoReembolso;
        saldoOrigen.FechaActualizacion = DateTime.UtcNow;

        orden.Estado = "Cancelada";
        orden.FechaCancelacion = DateTime.UtcNow;
        orden.FechaActualizacion = DateTime.UtcNow;

        _context.CancelacionesOrdenOferta.Add(new CancelacionesOrdenOferta
        {
            UsuarioId = usuarioId,
            TipoOperacion = "OrdenCompra",
            OrdenCompraId = ordenId,
            ParMonedaId = orden.ParMonedaId,
            CantidadEjecutada = orden.CantidadObtenida,
            CantidadCancelada = orden.CantidadPendiente,
            MontoReembolsado = montoReembolso,
            FechaCancelacion = DateTime.UtcNow
        });

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = usuarioId,
            MonedaId = orden.ParMoneda.MonedaOrigenId,
            TipoMovimiento = "DevolucionOrden",
            Monto = montoReembolso,
            SaldoAnterior = saldoAnterior,
            SaldoPosterior = saldoOrigen.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OrdenCompra",
            ReferenciaId = ordenId
        });

        _context.HistorialTransacciones.Add(new HistorialTransacciones
        {
            UsuarioId = usuarioId,
            TipoOperacion = "CancelacionOrden",
            ReferenciaId = ordenId,
            ParMonedaId = orden.ParMonedaId,
            FechaHora = DateTime.UtcNow,
            Estado = "Cancelada"
        });

        await _context.SaveChangesAsync();
    }

    public async Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId)
    {
        var compras = await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(
                g.Key,
                g.Sum(o => o.CantidadPendiente),
                g.Count()))
            .OrderByDescending(n => n.Precio)
            .Take(20)
            .ToListAsync();

        var ventas = await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(
                g.Key,
                g.Sum(o => o.CantidadPendiente),
                g.Count()))
            .OrderBy(n => n.Precio)
            .Take(20)
            .ToListAsync();

        return new LibroOrdenesDto(compras, ventas);
    }

    public async Task<LibroOrdenesDetalleDto> ObtenerLibroOrdenesDetalleAsync(int parMonedaId, int limite = 10)
    {
        var compras = await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .OrderByDescending(o => o.PrecioUnitario)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(
                o.OrdenCompraId, o.CantidadPendiente, o.PrecioUnitario, o.FechaCreacion))
            .ToListAsync();

        var ventas = await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .OrderBy(o => o.PrecioUnitario)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(
                o.OfertaVentaId, o.CantidadPendiente, o.PrecioUnitario, o.FechaCreacion))
            .ToListAsync();

        return new LibroOrdenesDetalleDto(compras, ventas);
    }

    private static OrdenDto MapOrdenDto(OrdenesCompra o, ParesMoneda par) =>
        new(o.OrdenCompraId, o.ParMonedaId,
            par.MonedaOrigen.CodigoIso, par.MonedaDestino.CodigoIso,
            o.CantidadOriginal, o.CantidadObtenida, o.CantidadPendiente,
            o.PrecioUnitario, o.TotalComprometido, o.TotalEjecutado,
            o.Estado, o.FechaCreacion, o.FechaActualizacion);
}
