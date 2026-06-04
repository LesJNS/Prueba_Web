using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class OperacionInmediataService : IOperacionInmediataService
{
    private readonly ExchangeDivisasDbContext _context;

    public OperacionInmediataService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<OperacionInmediataDto> EjecutarOperacionAsync(int usuarioId, OperacionInmediataRequest request)
    {
        if (request.Cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.");

        var tipoValido = request.TipoOperacion is "Compra" or "Venta";
        if (!tipoValido)
            throw new ArgumentException("TipoOperacion debe ser 'Compra' o 'Venta'.");

        var par = await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p => p.ParMonedaId == request.ParMonedaId)
            ?? throw new InvalidOperationException("Par de moneda no encontrado.");

        if (!par.Activo)
            throw new InvalidOperationException("El par de moneda está inactivo.");

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var operacion = new OperacionesInmediatas
        {
            UsuarioId = usuarioId,
            ParMonedaId = request.ParMonedaId,
            TipoOperacion = request.TipoOperacion,
            MetodoEjecucion = "Inmediata",
            CantidadSolicitada = request.Cantidad,
            CantidadEjecutada = 0,
            PrecioMinimo = request.PrecioMinimo,
            PrecioMaximo = request.PrecioMaximo,
            Estado = "Pendiente",
            FechaOperacion = DateTime.UtcNow
        };
        _context.OperacionesInmediatas.Add(operacion);
        await _context.SaveChangesAsync();

        var ejecuciones = new List<EjecucionesOrden>();
        decimal cantidadRestante = request.Cantidad;

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            if (request.TipoOperacion == "Compra")
            {
                var saldoOrigen = await _context.SaldosBilletera
                    .FirstOrDefaultAsync(s =>
                        s.BilleteraId == billetera.BilleteraId &&
                        s.MonedaId == par.MonedaOrigenId);

                var ofertas = _context.OfertasVenta
                    .AsQueryable()
                    .Where(o =>
                        o.ParMonedaId == request.ParMonedaId &&
                        o.Estado == "Activa" &&
                        o.CantidadPendiente > 0);

                if (request.PrecioMaximo.HasValue)
                    ofertas = ofertas.Where(o => o.PrecioUnitario <= request.PrecioMaximo.Value);

                var listaOfertas = await ofertas
                    .OrderBy(o => o.PrecioUnitario)
                    .ThenBy(o => o.FechaCreacion)
                    .ToListAsync();

                foreach (var oferta in listaOfertas)
                {
                    if (cantidadRestante <= 0) break;

                    var billeteraVendedor = await _context.Billeteras
                        .FirstOrDefaultAsync(b => b.UsuarioId == oferta.UsuarioId);
                    if (billeteraVendedor == null) continue;

                    var cantAEjecutar = Math.Min(cantidadRestante, oferta.CantidadPendiente);
                    var costoTotal = cantAEjecutar * oferta.PrecioUnitario;

                    if (saldoOrigen == null || saldoOrigen.SaldoDisponible < costoTotal) continue;

                    var ejecucion = await EjecutarFillAsync(
                        operacion, oferta, null,
                        billeteraVendedor, billetera,
                        par, cantAEjecutar, oferta.PrecioUnitario, saldoOrigen);

                    ejecuciones.Add(ejecucion);
                    cantidadRestante -= cantAEjecutar;
                }
            }
            else
            {
                var saldoDestino = await _context.SaldosBilletera
                    .FirstOrDefaultAsync(s =>
                        s.BilleteraId == billetera.BilleteraId &&
                        s.MonedaId == par.MonedaDestinoId);

                var ordenes = _context.OrdenesCompra
                    .AsQueryable()
                    .Where(o =>
                        o.ParMonedaId == request.ParMonedaId &&
                        o.Estado == "Activa" &&
                        o.CantidadPendiente > 0);

                if (request.PrecioMinimo.HasValue)
                    ordenes = ordenes.Where(o => o.PrecioUnitario >= request.PrecioMinimo.Value);

                var listaOrdenes = await ordenes
                    .OrderByDescending(o => o.PrecioUnitario)
                    .ThenBy(o => o.FechaCreacion)
                    .ToListAsync();

                foreach (var orden in listaOrdenes)
                {
                    if (cantidadRestante <= 0) break;

                    var billeteraComprador = await _context.Billeteras
                        .FirstOrDefaultAsync(b => b.UsuarioId == orden.UsuarioId);
                    if (billeteraComprador == null) continue;

                    var cantAEjecutar = Math.Min(cantidadRestante, orden.CantidadPendiente);

                    if (saldoDestino == null || saldoDestino.SaldoDisponible < cantAEjecutar) continue;

                    var ejecucion = await EjecutarFillVentaAsync(
                        operacion, orden, billetera, billeteraComprador,
                        par, cantAEjecutar, orden.PrecioUnitario, saldoDestino);

                    ejecuciones.Add(ejecucion);
                    cantidadRestante -= cantAEjecutar;
                }
            }

            operacion.CantidadEjecutada = request.Cantidad - cantidadRestante;
            operacion.Estado = operacion.CantidadEjecutada == 0 ? "Fallida"
                : cantidadRestante > 0 ? "Parcial"
                : "Completada";

            if (ejecuciones.Count > 0)
            {
                operacion.PrecioMinimo = ejecuciones.Min(e => e.PrecioUnitario);
                operacion.PrecioMaximo = ejecuciones.Max(e => e.PrecioUnitario);
                operacion.PrecioPromedio = ejecuciones.Sum(e => e.TotalOperacion) /
                                           ejecuciones.Sum(e => e.CantidadEjecutada);
                operacion.TotalPagado = ejecuciones.Sum(e => e.TotalOperacion);
                operacion.TotalRecibido = ejecuciones.Sum(e => e.CantidadEjecutada);
            }

            foreach (var ej in ejecuciones)
            {
                _context.OperacionInmediataEjecuciones.Add(new OperacionInmediataEjecuciones
                {
                    OperacionInmediataId = operacion.OperacionInmediataId,
                    EjecucionId = ej.EjecucionId
                });
            }

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "OperacionInmediata",
                ReferenciaId = operacion.OperacionInmediataId,
                ParMonedaId = request.ParMonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = operacion.Estado,
                MetodoEjecucion = "Inmediata"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            operacion.Estado = "Fallida";
            await _context.SaveChangesAsync();
            throw;
        }

        return await ObtenerOperacionAsync(usuarioId, operacion.OperacionInmediataId);
    }

    private async Task<EjecucionesOrden> EjecutarFillAsync(
        OperacionesInmediatas operacion,
        OfertasVenta oferta,
        OrdenesCompra? ordenRef,
        Billeteras billeteraVendedor,
        Billeteras billeteraComprador,
        ParesMoneda par,
        decimal cantidad,
        decimal precio,
        SaldosBilletera saldoOrigen)
    {
        var totalEjecucion = cantidad * precio;

        var ejecucion = new EjecucionesOrden
        {
            OrdenCompraId = ordenRef?.OrdenCompraId ?? 0,
            OfertaVentaId = oferta.OfertaVentaId,
            ParMonedaId = par.ParMonedaId,
            CompradorId = billeteraComprador.UsuarioId,
            VendedorId = billeteraVendedor.UsuarioId,
            CantidadEjecutada = cantidad,
            PrecioUnitario = precio,
            TotalOperacion = totalEjecucion,
            FechaEjecucion = DateTime.UtcNow
        };
        _context.EjecucionesOrden.Add(ejecucion);

        var saldoAnteriorOrigen = saldoOrigen.SaldoDisponible;
        saldoOrigen.SaldoDisponible -= totalEjecucion;
        saldoOrigen.FechaActualizacion = DateTime.UtcNow;

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = billeteraComprador.UsuarioId,
            MonedaId = par.MonedaOrigenId,
            TipoMovimiento = "CompraInmediata",
            Monto = -totalEjecucion,
            SaldoAnterior = saldoAnteriorOrigen,
            SaldoPosterior = saldoOrigen.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OperacionInmediata",
            ReferenciaId = operacion.OperacionInmediataId
        });

        var saldoDestinoComprador = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
            _context, billeteraComprador.BilleteraId, par.MonedaDestinoId);
        var saldoAnteriorDestinoComprador = saldoDestinoComprador.SaldoDisponible;
        saldoDestinoComprador.SaldoDisponible += cantidad;
        saldoDestinoComprador.FechaActualizacion = DateTime.UtcNow;

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = billeteraComprador.UsuarioId,
            MonedaId = par.MonedaDestinoId,
            TipoMovimiento = "CompraInmediataRecibido",
            Monto = cantidad,
            SaldoAnterior = saldoAnteriorDestinoComprador,
            SaldoPosterior = saldoDestinoComprador.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OperacionInmediata",
            ReferenciaId = operacion.OperacionInmediataId
        });

        var saldoOrigenVendedor = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
            _context, billeteraVendedor.BilleteraId, par.MonedaOrigenId);
        var saldoAnteriorOrigenVendedor = saldoOrigenVendedor.SaldoDisponible;
        saldoOrigenVendedor.SaldoDisponible += totalEjecucion;
        saldoOrigenVendedor.FechaActualizacion = DateTime.UtcNow;

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = billeteraVendedor.UsuarioId,
            MonedaId = par.MonedaOrigenId,
            TipoMovimiento = "VentaInmediata",
            Monto = totalEjecucion,
            SaldoAnterior = saldoAnteriorOrigenVendedor,
            SaldoPosterior = saldoOrigenVendedor.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OperacionInmediata",
            ReferenciaId = operacion.OperacionInmediataId
        });

        oferta.CantidadVendida += cantidad;
        oferta.CantidadPendiente -= cantidad;
        oferta.TotalRecibido += totalEjecucion;
        oferta.FechaActualizacion = DateTime.UtcNow;
        if (oferta.CantidadPendiente == 0) oferta.Estado = "Completada";

        await _context.SaveChangesAsync();
        return ejecucion;
    }

    private async Task<EjecucionesOrden> EjecutarFillVentaAsync(
        OperacionesInmediatas operacion,
        OrdenesCompra orden,
        Billeteras billeteraVendedor,
        Billeteras billeteraComprador,
        ParesMoneda par,
        decimal cantidad,
        decimal precio,
        SaldosBilletera saldoDestinoVendedor)
    {
        var totalEjecucion = cantidad * precio;

        var ejecucion = new EjecucionesOrden
        {
            OrdenCompraId = orden.OrdenCompraId,
            OfertaVentaId = 0,
            ParMonedaId = par.ParMonedaId,
            CompradorId = billeteraComprador.UsuarioId,
            VendedorId = billeteraVendedor.UsuarioId,
            CantidadEjecutada = cantidad,
            PrecioUnitario = precio,
            TotalOperacion = totalEjecucion,
            FechaEjecucion = DateTime.UtcNow
        };
        _context.EjecucionesOrden.Add(ejecucion);

        var saldoAnteriorDestinoVendedor = saldoDestinoVendedor.SaldoDisponible;
        saldoDestinoVendedor.SaldoDisponible -= cantidad;
        saldoDestinoVendedor.FechaActualizacion = DateTime.UtcNow;

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = billeteraVendedor.UsuarioId,
            MonedaId = par.MonedaDestinoId,
            TipoMovimiento = "VentaInmediata",
            Monto = -cantidad,
            SaldoAnterior = saldoAnteriorDestinoVendedor,
            SaldoPosterior = saldoDestinoVendedor.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OperacionInmediata",
            ReferenciaId = operacion.OperacionInmediataId
        });

        var saldoOrigenVendedor = await BilleteraService.ObtenerOCrearSaldoInternoAsync(
            _context, billeteraVendedor.BilleteraId, par.MonedaOrigenId);
        var saldoAnteriorOrigenVendedor = saldoOrigenVendedor.SaldoDisponible;
        saldoOrigenVendedor.SaldoDisponible += totalEjecucion;
        saldoOrigenVendedor.FechaActualizacion = DateTime.UtcNow;

        _context.MovimientosBilletera.Add(new MovimientosBilletera
        {
            UsuarioId = billeteraVendedor.UsuarioId,
            MonedaId = par.MonedaOrigenId,
            TipoMovimiento = "VentaInmediataRecibido",
            Monto = totalEjecucion,
            SaldoAnterior = saldoAnteriorOrigenVendedor,
            SaldoPosterior = saldoOrigenVendedor.SaldoDisponible,
            FechaMovimiento = DateTime.UtcNow,
            ReferenciaTipo = "OperacionInmediata",
            ReferenciaId = operacion.OperacionInmediataId
        });

        orden.CantidadObtenida += totalEjecucion;
        orden.CantidadPendiente -= cantidad;
        orden.TotalEjecutado += totalEjecucion;
        orden.FechaActualizacion = DateTime.UtcNow;
        orden.Estado = orden.CantidadPendiente == 0 ? "Completada" : "Parcial";

        await _context.SaveChangesAsync();
        return ejecucion;
    }

    public async Task<List<OperacionInmediataDto>> ObtenerMisOperacionesAsync(int usuarioId)
    {
        var ops = await _context.OperacionesInmediatas
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.FechaOperacion)
            .ToListAsync();

        return ops.Select(o => MapDto(o, null)).ToList();
    }

    public async Task<OperacionInmediataDto> ObtenerOperacionAsync(int usuarioId, int operacionId)
    {
        var op = await _context.OperacionesInmediatas
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .Include(o => o.OperacionInmediataEjecuciones)
            .ThenInclude(oe => oe.Ejecucion)
            .FirstOrDefaultAsync(o => o.OperacionInmediataId == operacionId && o.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Operación no encontrada.");

        var ejecuciones = op.OperacionInmediataEjecuciones
            .Select(oe => new EjecucionDto(
                oe.Ejecucion.EjecucionId,
                op.ParMonedaId,
                op.ParMoneda.MonedaOrigen.CodigoIso,
                op.ParMoneda.MonedaDestino.CodigoIso,
                oe.Ejecucion.CantidadEjecutada,
                oe.Ejecucion.PrecioUnitario,
                oe.Ejecucion.TotalOperacion,
                oe.Ejecucion.FechaEjecucion))
            .ToList();

        return MapDto(op, ejecuciones);
    }

    private static OperacionInmediataDto MapDto(OperacionesInmediatas o, List<EjecucionDto>? ejecuciones) =>
        new(o.OperacionInmediataId, o.ParMonedaId,
            o.ParMoneda.MonedaOrigen.CodigoIso, o.ParMoneda.MonedaDestino.CodigoIso,
            o.TipoOperacion, o.MetodoEjecucion,
            o.CantidadSolicitada, o.CantidadEjecutada,
            o.PrecioMinimo, o.PrecioMaximo, o.PrecioPromedio,
            o.TotalPagado, o.TotalRecibido,
            o.Estado, o.FechaOperacion, ejecuciones);
}
