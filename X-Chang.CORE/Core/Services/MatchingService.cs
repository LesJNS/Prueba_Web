using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class MatchingService : IMatchingService
{
    private readonly ExchangeDivisasDbContext _context;

    public MatchingService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task EjecutarMatchingOrdenAsync(int ordenCompraId)
    {
        var orden = await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
            .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenCompraId);

        if (orden == null || orden.Estado == "Cancelada" || orden.CantidadPendiente <= 0)
            return;

        var ofertasCompatibles = await _context.OfertasVenta
            .Where(o =>
                o.ParMonedaId == orden.ParMonedaId &&
                o.Estado == "Activa" &&
                o.PrecioUnitario <= orden.PrecioUnitario &&
                o.CantidadPendiente > 0)
            .OrderBy(o => o.PrecioUnitario)
            .ThenBy(o => o.FechaCreacion)
            .ToListAsync();

        var billeteraComprador = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == orden.UsuarioId);

        if (billeteraComprador == null) return;

        foreach (var oferta in ofertasCompatibles)
        {
            if (orden.CantidadPendiente <= 0) break;

            var billeteraVendedor = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == oferta.UsuarioId);
            if (billeteraVendedor == null) continue;

            var cantidadAEjecutar = Math.Min(orden.CantidadPendiente, oferta.CantidadPendiente);
            var precioEjecucion = oferta.PrecioUnitario;
            var totalEjecucion = cantidadAEjecutar * precioEjecucion;
            var excessoComprador = cantidadAEjecutar * (orden.PrecioUnitario - precioEjecucion);

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var ejecucion = new EjecucionesOrden
                {
                    OrdenCompraId = orden.OrdenCompraId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    ParMonedaId = orden.ParMonedaId,
                    CompradorId = orden.UsuarioId,
                    VendedorId = oferta.UsuarioId,
                    CantidadEjecutada = cantidadAEjecutar,
                    PrecioUnitario = precioEjecucion,
                    TotalOperacion = totalEjecucion,
                    FechaEjecucion = DateTime.UtcNow
                };
                _context.EjecucionesOrden.Add(ejecucion);

                orden.CantidadObtenida += cantidadAEjecutar;
                orden.CantidadPendiente -= cantidadAEjecutar;
                orden.TotalEjecutado += totalEjecucion;
                orden.FechaActualizacion = DateTime.UtcNow;
                orden.Estado = orden.CantidadPendiente == 0 ? "Completada" : "Parcial";

                oferta.CantidadVendida += cantidadAEjecutar;
                oferta.CantidadPendiente -= cantidadAEjecutar;
                oferta.TotalRecibido += totalEjecucion;
                oferta.FechaActualizacion = DateTime.UtcNow;
                if (oferta.CantidadPendiente == 0) oferta.Estado = "Completada";

                var saldoCompradorDestino = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
                    _context, billeteraComprador.BilleteraId, orden.ParMoneda.MonedaDestinoId);
                var saldoAnteriorCompradorDestino = saldoCompradorDestino.SaldoDisponible;
                saldoCompradorDestino.SaldoDisponible += cantidadAEjecutar;
                saldoCompradorDestino.FechaActualizacion = DateTime.UtcNow;

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = orden.UsuarioId,
                    MonedaId = orden.ParMoneda.MonedaDestinoId,
                    TipoMovimiento = "Compra",
                    Monto = cantidadAEjecutar,
                    SaldoAnterior = saldoAnteriorCompradorDestino,
                    SaldoPosterior = saldoCompradorDestino.SaldoDisponible,
                    FechaMovimiento = DateTime.UtcNow,
                    ReferenciaTipo = "EjecucionOrden"
                });

                if (excessoComprador > 0)
                {
                    var saldoCompradorOrigen = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
                        _context, billeteraComprador.BilleteraId, orden.ParMoneda.MonedaOrigenId);
                    var saldoAnteriorOrigenComprador = saldoCompradorOrigen.SaldoDisponible;
                    saldoCompradorOrigen.SaldoDisponible += excessoComprador;
                    saldoCompradorOrigen.FechaActualizacion = DateTime.UtcNow;

                    _context.MovimientosBilletera.Add(new MovimientosBilletera
                    {
                        UsuarioId = orden.UsuarioId,
                        MonedaId = orden.ParMoneda.MonedaOrigenId,
                        TipoMovimiento = "DevolucionPrecio",
                        Monto = excessoComprador,
                        SaldoAnterior = saldoAnteriorOrigenComprador,
                        SaldoPosterior = saldoCompradorOrigen.SaldoDisponible,
                        FechaMovimiento = DateTime.UtcNow,
                        ReferenciaTipo = "EjecucionOrden"
                    });
                }

                var saldoVendedorOrigen = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
                    _context, billeteraVendedor.BilleteraId, orden.ParMoneda.MonedaOrigenId);
                var saldoAnteriorVendedorOrigen = saldoVendedorOrigen.SaldoDisponible;
                saldoVendedorOrigen.SaldoDisponible += totalEjecucion;
                saldoVendedorOrigen.FechaActualizacion = DateTime.UtcNow;

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = oferta.UsuarioId,
                    MonedaId = orden.ParMoneda.MonedaOrigenId,
                    TipoMovimiento = "Venta",
                    Monto = totalEjecucion,
                    SaldoAnterior = saldoAnteriorVendedorOrigen,
                    SaldoPosterior = saldoVendedorOrigen.SaldoDisponible,
                    FechaMovimiento = DateTime.UtcNow,
                    ReferenciaTipo = "EjecucionOrden"
                });

                await _context.SaveChangesAsync();

                var ejecucionId = ejecucion.EjecucionId;
                _context.MovimientosBilletera.Local
                    .Where(m => m.ReferenciaId == null && m.ReferenciaTipo == "EjecucionOrden")
                    .ToList()
                    .ForEach(m => m.ReferenciaId = ejecucionId);

                _context.HistorialTransacciones.Add(new HistorialTransacciones
                {
                    UsuarioId = orden.UsuarioId,
                    TipoOperacion = "OrdenCompra",
                    ReferenciaId = ejecucionId,
                    ParMonedaId = orden.ParMonedaId,
                    FechaHora = DateTime.UtcNow,
                    Estado = "Ejecutada"
                });

                _context.HistorialTransacciones.Add(new HistorialTransacciones
                {
                    UsuarioId = oferta.UsuarioId,
                    TipoOperacion = "OfertaVenta",
                    ReferenciaId = ejecucionId,
                    ParMonedaId = oferta.ParMonedaId,
                    FechaHora = DateTime.UtcNow,
                    Estado = "Ejecutada"
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
    }

    public async Task EjecutarMatchingOfertaAsync(int ofertaVentaId)
    {
        var oferta = await _context.OfertasVenta
            .Include(o => o.ParMoneda)
            .FirstOrDefaultAsync(o => o.OfertaVentaId == ofertaVentaId);

        if (oferta == null || oferta.Estado == "Cancelada" || oferta.CantidadPendiente <= 0)
            return;

        var ordenesCompatibles = await _context.OrdenesCompra
            .Where(o =>
                o.ParMonedaId == oferta.ParMonedaId &&
                o.Estado == "Activa" &&
                o.PrecioUnitario >= oferta.PrecioUnitario &&
                o.CantidadPendiente > 0)
            .OrderByDescending(o => o.PrecioUnitario)
            .ThenBy(o => o.FechaCreacion)
            .ToListAsync();

        foreach (var orden in ordenesCompatibles)
        {
            if (oferta.CantidadPendiente <= 0) break;
            await EjecutarMatchingOrdenAsync(orden.OrdenCompraId);

            await _context.Entry(oferta).ReloadAsync();
        }
    }
}
