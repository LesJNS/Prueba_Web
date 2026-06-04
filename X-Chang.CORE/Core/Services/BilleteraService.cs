using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class BilleteraService : IBilleteraService
{
    private readonly ExchangeDivisasDbContext _context;

    public BilleteraService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<BilleteraDto> ObtenerBilleteraAsync(int usuarioId)
    {
        var billetera = await _context.Billeteras
            .Include(b => b.SaldosBilletera)
            .ThenInclude(s => s.Moneda)
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldos = billetera.SaldosBilletera
            .Select(s => new SaldoDto(
                s.SaldoId,
                s.Moneda.CodigoIso,
                s.Moneda.Nombre,
                s.Moneda.Tipo,
                s.SaldoDisponible,
                s.FechaActualizacion))
            .ToList();

        return new BilleteraDto(
            billetera.BilleteraId,
            billetera.UsuarioId,
            billetera.FechaCreacion,
            saldos);
    }

    public async Task<DepositoDto> DepositarAsync(int usuarioId, DepositoRequest request)
    {
        if (request.Monto <= 0)
            throw new ArgumentException("El monto debe ser mayor a cero.");

        var metodoPago = await _context.MetodosPago.FindAsync(request.MetodoPagoId)
            ?? throw new InvalidOperationException("Método de pago no encontrado.");

        if (!metodoPago.Activo)
            throw new InvalidOperationException("Método de pago inactivo.");

        var moneda = await _context.Monedas.FindAsync(request.MonedaId)
            ?? throw new InvalidOperationException("Moneda no encontrada.");

        if (!moneda.Activa)
            throw new InvalidOperationException("Moneda inactiva.");

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var comision = Math.Max(
            request.Monto * metodoPago.ComisionPorcentaje / 100m,
            metodoPago.ComisionFija);
        var totalPagado = request.Monto + comision;

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var deposito = new Depositos
            {
                UsuarioId = usuarioId,
                MonedaId = request.MonedaId,
                MetodoPagoId = request.MetodoPagoId,
                MontoDepositado = request.Monto,
                ComisionAplicada = comision,
                TotalPagado = totalPagado,
                Estado = "Completada",
                VoucherUrl = request.VoucherUrl,
                FechaDeposito = DateTime.UtcNow
            };
            _context.Depositos.Add(deposito);
            await _context.SaveChangesAsync();

            var saldo = await ObtenerOCrearSaldoAsync(billetera.BilleteraId, request.MonedaId);
            var saldoAnterior = saldo.SaldoDisponible;
            saldo.SaldoDisponible += request.Monto;
            saldo.FechaActualizacion = DateTime.UtcNow;

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = request.MonedaId,
                TipoMovimiento = "Deposito",
                Monto = request.Monto,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldo.SaldoDisponible,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "Deposito",
                ReferenciaId = deposito.DepositoId
            });

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Deposito",
                ReferenciaId = deposito.DepositoId,
                MonedaId = request.MonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new DepositoDto(
                deposito.DepositoId, moneda.MonedaId, moneda.Nombre, metodoPago.Nombre,
                deposito.MontoDepositado, deposito.ComisionAplicada, deposito.TotalPagado,
                deposito.Estado, deposito.VoucherUrl, deposito.FechaDeposito);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<RetiroDto> RetirarAsync(int usuarioId, RetiroRequest request)
    {
        if (request.Monto <= 0)
            throw new ArgumentException("El monto debe ser mayor a cero.");

        var metodoPago = await _context.MetodosPago.FindAsync(request.MetodoPagoId)
            ?? throw new InvalidOperationException("Método de pago no encontrado.");

        if (!metodoPago.Activo)
            throw new InvalidOperationException("Método de pago inactivo.");

        var moneda = await _context.Monedas.FindAsync(request.MonedaId)
            ?? throw new InvalidOperationException("Moneda no encontrada.");

        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldo = await _context.SaldosBilletera
            .FirstOrDefaultAsync(s =>
                s.BilleteraId == billetera.BilleteraId &&
                s.MonedaId == request.MonedaId);

        if (saldo == null || saldo.SaldoDisponible < request.Monto)
            throw new InvalidOperationException("Saldo insuficiente.");

        var comision = Math.Max(
            request.Monto * metodoPago.ComisionPorcentaje / 100m,
            metodoPago.ComisionFija);
        var montoFinal = request.Monto - comision;

        if (montoFinal <= 0)
            throw new InvalidOperationException("El monto no cubre las comisiones del método de pago.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var saldoAnterior = saldo.SaldoDisponible;
            saldo.SaldoDisponible -= request.Monto;
            saldo.FechaActualizacion = DateTime.UtcNow;

            var retiro = new Retiros
            {
                UsuarioId = usuarioId,
                MonedaId = request.MonedaId,
                MetodoPagoId = request.MetodoPagoId,
                MontoRetirado = request.Monto,
                ComisionAplicada = comision,
                MontoFinalRecibido = montoFinal,
                Estado = "Completada",
                VoucherUrl = request.VoucherUrl,
                FechaRetiro = DateTime.UtcNow
            };
            _context.Retiros.Add(retiro);
            await _context.SaveChangesAsync();

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = request.MonedaId,
                TipoMovimiento = "Retiro",
                Monto = -request.Monto,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldo.SaldoDisponible,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "Retiro",
                ReferenciaId = retiro.RetiroId
            });

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Retiro",
                ReferenciaId = retiro.RetiroId,
                MonedaId = request.MonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new RetiroDto(
                retiro.RetiroId, moneda.MonedaId, moneda.Nombre, metodoPago.Nombre,
                retiro.MontoRetirado, retiro.ComisionAplicada, retiro.MontoFinalRecibido,
                retiro.Estado, retiro.VoucherUrl, retiro.FechaRetiro);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<PagedResult<MovimientoDto>> ObtenerMovimientosAsync(int usuarioId, int pagina, int tamano)
    {
        var query = _context.MovimientosBilletera
            .Include(m => m.Moneda)
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaMovimiento);

        var total = await query.CountAsync();
        var items = await query
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(m => new MovimientoDto(
                m.MovimientoId,
                m.Moneda.CodigoIso,
                m.Moneda.Nombre,
                m.TipoMovimiento,
                m.Monto,
                m.SaldoAnterior,
                m.SaldoPosterior,
                m.FechaMovimiento,
                m.ReferenciaTipo,
                m.ReferenciaId))
            .ToListAsync();

        return new PagedResult<MovimientoDto>(items, total, pagina, tamano);
    }

    public async Task<List<DepositoDto>> ObtenerDepositosAsync(int usuarioId)
    {
        return await _context.Depositos
            .Include(d => d.Moneda)
            .Include(d => d.MetodoPago)
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.FechaDeposito)
            .Select(d => new DepositoDto(
                d.DepositoId, d.MonedaId, d.Moneda.Nombre, d.MetodoPago.Nombre,
                d.MontoDepositado, d.ComisionAplicada, d.TotalPagado,
                d.Estado, d.VoucherUrl, d.FechaDeposito))
            .ToListAsync();
    }

    public async Task<List<RetiroDto>> ObtenerRetirosAsync(int usuarioId)
    {
        return await _context.Retiros
            .Include(r => r.Moneda)
            .Include(r => r.MetodoPago)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRetiro)
            .Select(r => new RetiroDto(
                r.RetiroId, r.MonedaId, r.Moneda.Nombre, r.MetodoPago.Nombre,
                r.MontoRetirado, r.ComisionAplicada, r.MontoFinalRecibido,
                r.Estado, r.VoucherUrl, r.FechaRetiro))
            .ToListAsync();
    }

    public async Task<List<MetodoPagoDto>> ObtenerMetodosPagoAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        return await _context.MetodosPagoPais
            .Include(mp => mp.MetodoPago)
            .Where(mp => mp.PaisId == usuario.PaisId && mp.Activo && mp.MetodoPago.Activo)
            .Select(mp => new MetodoPagoDto(
                mp.MetodoPagoId, mp.MetodoPago.Nombre, mp.MetodoPago.Tipo,
                mp.MetodoPago.ComisionPorcentaje, mp.MetodoPago.ComisionFija))
            .ToListAsync();
    }

    internal static async Task<SaldosBilletera> ObtenerOCrearSaldoInternoAsync(
        ExchangeDivisasDbContext context, int billeteraId, int monedaId)
    {
        var saldo = await context.SaldosBilletera
            .FirstOrDefaultAsync(s => s.BilleteraId == billeteraId && s.MonedaId == monedaId);

        if (saldo != null) return saldo;

        saldo = new SaldosBilletera
        {
            BilleteraId = billeteraId,
            MonedaId = monedaId,
            SaldoDisponible = 0,
            FechaActualizacion = DateTime.UtcNow
        };
        context.SaldosBilletera.Add(saldo);
        return saldo;
    }

    private Task<SaldosBilletera> ObtenerOCrearSaldoAsync(int billeteraId, int monedaId)
        => ObtenerOCrearSaldoInternoAsync(_context, billeteraId, monedaId);
}
