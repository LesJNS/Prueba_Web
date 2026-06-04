using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;

namespace X_Chang.CORE.Repositories;

public class BilleteraRepository : IBilleteraRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public BilleteraRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<Billeteras?> ObtenerPorUsuarioAsync(int usuarioId) =>
        await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

    public async Task<Billeteras?> ObtenerConSaldosAsync(int usuarioId) =>
        await _context.Billeteras
            .Include(b => b.SaldosBilletera).ThenInclude(s => s.Moneda)
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

    public async Task<SaldosBilletera?> ObtenerSaldoAsync(int billeteraId, int monedaId) =>
        await _context.SaldosBilletera
            .FirstOrDefaultAsync(s => s.BilleteraId == billeteraId && s.MonedaId == monedaId);

    public async Task<SaldosBilletera> ObtenerOCrearSaldoAsync(int billeteraId, int monedaId)
    {
        var saldo = await _context.SaldosBilletera
            .FirstOrDefaultAsync(s => s.BilleteraId == billeteraId && s.MonedaId == monedaId);

        if (saldo != null) return saldo;

        saldo = new SaldosBilletera
        {
            BilleteraId = billeteraId,
            MonedaId = monedaId,
            SaldoDisponible = 0,
            FechaActualizacion = DateTime.UtcNow
        };
        _context.SaldosBilletera.Add(saldo);
        return saldo;
    }

    public async Task<MetodosPago?> ObtenerMetodoPagoPorIdAsync(int metodoPagoId) =>
        await _context.MetodosPago.FindAsync(metodoPagoId);

    public async Task<Monedas?> ObtenerMonedaPorIdAsync(int monedaId) =>
        await _context.Monedas.FindAsync(monedaId);

    public async Task<List<MetodosPagoPais>> ObtenerMetodosPorPaisAsync(int paisId) =>
        await _context.MetodosPagoPais
            .Include(mp => mp.MetodoPago)
            .Where(mp => mp.PaisId == paisId && mp.Activo && mp.MetodoPago.Activo)
            .ToListAsync();

    public async Task<(List<MovimientosBilletera> Items, int Total)> ObtenerMovimientosAsync(
        int usuarioId, int pagina, int tamano)
    {
        var query = _context.MovimientosBilletera
            .Include(m => m.Moneda)
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaMovimiento);

        var total = await query.CountAsync();
        var items = await query.Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
        return (items, total);
    }

    public async Task<List<Depositos>> ObtenerDepositosAsync(int usuarioId) =>
        await _context.Depositos
            .Include(d => d.Moneda).Include(d => d.MetodoPago)
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.FechaDeposito)
            .ToListAsync();

    public async Task<List<Retiros>> ObtenerRetirosAsync(int usuarioId) =>
        await _context.Retiros
            .Include(r => r.Moneda).Include(r => r.MetodoPago)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaRetiro)
            .ToListAsync();

    public Task AgregarDepositoAsync(Depositos deposito) { _context.Depositos.Add(deposito); return Task.CompletedTask; }
    public Task AgregarRetiroAsync(Retiros retiro) { _context.Retiros.Add(retiro); return Task.CompletedTask; }
    public Task AgregarMovimientoAsync(MovimientosBilletera movimiento) { _context.MovimientosBilletera.Add(movimiento); return Task.CompletedTask; }
    public async Task GuardarCambiosAsync() => await _context.SaveChangesAsync();
}
