using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class HistorialService : IHistorialService
{
    private readonly ExchangeDivisasDbContext _context;

    public HistorialService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<HistorialDto>> ObtenerHistorialAsync(
        int usuarioId, FiltroHistorialRequest filtro)
    {
        var query = _context.HistorialTransacciones
            .Include(h => h.ParMoneda)
            .ThenInclude(p => p!.MonedaOrigen)
            .Include(h => h.ParMoneda)
            .ThenInclude(p => p!.MonedaDestino)
            .Include(h => h.Moneda)
            .Where(h => h.UsuarioId == usuarioId);

        if (filtro.Desde.HasValue)
            query = query.Where(h => h.FechaHora >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(h => h.FechaHora <= filtro.Hasta.Value);

        if (!string.IsNullOrWhiteSpace(filtro.TipoOperacion))
            query = query.Where(h => h.TipoOperacion == filtro.TipoOperacion);

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(h => h.Estado == filtro.Estado);

        query = query.OrderByDescending(h => h.FechaHora);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(h => new HistorialDto(
                h.HistorialId,
                h.TipoOperacion,
                h.ReferenciaId,
                h.ParMoneda != null
                    ? h.ParMoneda.MonedaOrigen.CodigoIso + "/" + h.ParMoneda.MonedaDestino.CodigoIso
                    : null,
                h.Moneda != null ? h.Moneda.CodigoIso : null,
                h.FechaHora,
                h.Estado,
                h.MetodoEjecucion))
            .ToListAsync();

        return new PagedResult<HistorialDto>(items, total, filtro.Pagina, filtro.TamanoPagina);
    }
}
