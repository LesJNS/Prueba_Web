using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;

namespace X_Chang.CORE.Services;

public static class BilleteraService
{
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
}
