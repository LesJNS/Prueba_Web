using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class ParesMonedasService : IParesMonedasService
{
    private readonly ExchangeDivisasDbContext _context;

    public ParesMonedasService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    private static readonly Dictionary<string, string> MonedaPorPais = new()
    {
        ["Estados Unidos"] = "USD", ["Alemania"] = "EUR", ["Reino Unido"] = "GBP",
        ["Suiza"] = "CHF", ["Japón"] = "JPY", ["Hong Kong"] = "HKD",
        ["Canadá"] = "CAD", ["China"] = "CNY", ["Australia"] = "AUD",
        ["Rusia"] = "RUB", ["Argentina"] = "ARS", ["Bolivia"] = "BOB",
        ["Brasil"] = "BRL", ["Chile"] = "CLP", ["Colombia"] = "COP",
        ["Costa Rica"] = "CRC", ["Cuba"] = "CUP", ["Guatemala"] = "GTQ",
        ["Honduras"] = "HNL", ["México"] = "MXN", ["Nicaragua"] = "NIO",
        ["Panamá"] = "PAB", ["Paraguay"] = "PYG", ["Perú"] = "PEN",
        ["República Dominicana"] = "DOP", ["Uruguay"] = "UYU", ["Venezuela"] = "VES"
    };

    public async Task<PagedResult<ParMonedasDto>> ObtenerParesAsync(FiltroParesRequest filtro, int? usuarioId)
    {
        var query = _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .Include(p => p.HistoricoPreciosPar.OrderByDescending(h => h.FechaRegistro).Take(1))
            .Where(p => p.Activo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.MonedaOrigen) && filtro.MonedaOrigen != "Cualquiera")
            query = query.Where(p => p.MonedaOrigen.CodigoIso == filtro.MonedaOrigen);

        if (!string.IsNullOrWhiteSpace(filtro.MonedaDestino) && filtro.MonedaDestino != "Cualquiera")
            query = query.Where(p => p.MonedaDestino.CodigoIso == filtro.MonedaDestino);

        var pares = await query.ToListAsync();

        if (filtro.ColapsarInversos)
        {
            pares = pares
                .GroupBy(p => string.Compare(p.MonedaOrigen.CodigoIso, p.MonedaDestino.CodigoIso) < 0
                    ? $"{p.MonedaOrigen.CodigoIso}/{p.MonedaDestino.CodigoIso}"
                    : $"{p.MonedaDestino.CodigoIso}/{p.MonedaOrigen.CodigoIso}")
                .Select(g => g.First())
                .ToList();
        }

        HashSet<int>? paresEnHistorial = null;
        if (usuarioId.HasValue && filtro.OrdenarPor == "FechaReciente")
        {
            var historial = await _context.HistorialTransacciones
                .Where(h => h.UsuarioId == usuarioId.Value && h.ParMonedaId != null)
                .Select(h => h.ParMonedaId)
                .ToListAsync();
            paresEnHistorial = historial.Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();
        }

        var dtos = pares.Select(p =>
        {
            var hist = p.HistoricoPreciosPar.OrderByDescending(h => h.FechaRegistro).FirstOrDefault();
            return new ParMonedasDto(
                p.ParMonedaId,
                p.MonedaOrigen.CodigoIso,
                p.MonedaDestino.CodigoIso,
                $"{p.MonedaOrigen.CodigoIso}/{p.MonedaDestino.CodigoIso}",
                hist?.MayorPrecioCompra,
                hist?.MenorPrecioVenta,
                hist?.Margen,
                (hist?.VolumenCompra ?? 0) + (hist?.VolumenVenta ?? 0),
                hist?.FechaRegistro);
        }).ToList();

        dtos = (filtro.OrdenarPor, filtro.Direccion) switch
        {
            ("FechaReciente", _) when paresEnHistorial != null => dtos
                .OrderBy(d => paresEnHistorial.Contains(d.ParMonedaId) ? 0 : 1)
                .ThenBy(d => filtro.Direccion == "Asc" ? d.UltimaTransaccion : DateTime.MinValue)
                .ThenByDescending(d => filtro.Direccion == "Desc" ? d.UltimaTransaccion : null)
                .ThenBy(d => d.Par)
                .ToList(),
            ("MayorPrecioCompra", "Asc") => dtos.OrderBy(d => d.MayorPrecioCompra).ToList(),
            ("MayorPrecioCompra", _) => dtos.OrderByDescending(d => d.MayorPrecioCompra).ToList(),
            ("MenorPrecioVenta", "Asc") => dtos.OrderBy(d => d.MenorPrecioVenta).ToList(),
            ("MenorPrecioVenta", _) => dtos.OrderByDescending(d => d.MenorPrecioVenta).ToList(),
            ("Margen", "Asc") => dtos.OrderBy(d => d.Margen).ToList(),
            ("Margen", _) => dtos.OrderByDescending(d => d.Margen).ToList(),
            ("Volumen", "Asc") => dtos.OrderBy(d => d.VolumenTotal).ToList(),
            ("Volumen", _) => dtos.OrderByDescending(d => d.VolumenTotal).ToList(),
            (_, "Asc") => dtos.OrderBy(d => d.Par).ToList(),
            _ => dtos.OrderBy(d => d.Par).ToList()
        };

        var total = dtos.Count;
        var items = dtos
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToList();

        return new PagedResult<ParMonedasDto>(items, total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<ParMonedasDetalleDto> ObtenerParDetalleAsync(int parMonedaId, FiltroHistoricoRequest filtro)
    {
        var par = await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId)
            ?? throw new InvalidOperationException("Par de monedas no encontrado.");

        var desde = filtro.RangoTemporal switch
        {
            "UltimoDia" => DateTime.UtcNow.AddDays(-1),
            "UltimaSemana" => DateTime.UtcNow.AddDays(-7),
            "UltimoMes" => DateTime.UtcNow.AddMonths(-1),
            "UltimoAnio" => DateTime.UtcNow.AddYears(-1),
            _ => DateTime.MinValue
        };

        var historico = await _context.HistoricoPreciosPar
            .Where(h => h.ParMonedaId == parMonedaId && h.FechaRegistro >= desde)
            .OrderBy(h => h.FechaRegistro)
            .Select(h => new HistoricoParDto(
                h.FechaRegistro, h.MayorPrecioCompra, h.MenorPrecioVenta,
                h.Margen, h.VolumenCompra, h.VolumenVenta))
            .ToListAsync();

        var ultimo = historico.LastOrDefault();

        return new ParMonedasDetalleDto(
            par.ParMonedaId,
            par.MonedaOrigen.CodigoIso,
            par.MonedaDestino.CodigoIso,
            $"{par.MonedaOrigen.CodigoIso}/{par.MonedaDestino.CodigoIso}",
            ultimo?.MayorPrecioCompra,
            ultimo?.MenorPrecioVenta,
            ultimo?.Margen,
            historico);
    }

    public async Task<GraficoPrincipalDto> ObtenerGraficoPrincipalAsync(int? usuarioId)
    {
        var filtroDefault = new FiltroHistoricoRequest("UltimoDia");
        ParMonedasDetalleDto primerGrafico;
        ParMonedasDetalleDto? segundoGrafico = null;

        if (!usuarioId.HasValue)
        {
            var parUsdEur = await ObtenerParPorCodigosAsync("USD", "EUR");
            primerGrafico = await ObtenerParDetalleAsync(parUsdEur.ParMonedaId, filtroDefault);
            return new GraficoPrincipalDto(primerGrafico, null);
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Pais)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId.Value)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        var monedaPrincipal = MonedaPorPais.GetValueOrDefault(usuario.Pais.Nombre, "USD");

        if (monedaPrincipal == "USD")
        {
            var parUsdEur = await ObtenerParPorCodigosAsync("USD", "EUR");
            primerGrafico = await ObtenerParDetalleAsync(parUsdEur.ParMonedaId, filtroDefault);
        }
        else
        {
            var parPrincipal = await ObtenerParPorCodigosAsync(monedaPrincipal, "USD");
            primerGrafico = await ObtenerParDetalleAsync(parPrincipal.ParMonedaId, filtroDefault);
        }

        var ordenReciente = await _context.OrdenesCompra
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId.Value && o.Estado == "Activa")
            .OrderByDescending(o => o.FechaCreacion)
            .FirstOrDefaultAsync();

        var ofertaReciente = await _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId.Value && o.Estado == "Activa")
            .OrderByDescending(o => o.FechaCreacion)
            .FirstOrDefaultAsync();

        int? segundoParId = null;
        if (ordenReciente != null && ofertaReciente != null)
        {
            segundoParId = ordenReciente.FechaCreacion > ofertaReciente.FechaCreacion
                ? ordenReciente.ParMonedaId : ofertaReciente.ParMonedaId;
        }
        else if (ordenReciente != null) segundoParId = ordenReciente.ParMonedaId;
        else if (ofertaReciente != null) segundoParId = ofertaReciente.ParMonedaId;

        if (segundoParId.HasValue && segundoParId != primerGrafico.ParMonedaId)
        {
            segundoGrafico = await ObtenerParDetalleAsync(segundoParId.Value, filtroDefault);
        }
        else
        {
            var parInverso = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p =>
                    p.MonedaOrigen.CodigoIso == primerGrafico.MonedaDestino &&
                    p.MonedaDestino.CodigoIso == primerGrafico.MonedaOrigen &&
                    p.Activo);
            if (parInverso != null)
                segundoGrafico = await ObtenerParDetalleAsync(parInverso.ParMonedaId, filtroDefault);
        }

        return new GraficoPrincipalDto(primerGrafico, segundoGrafico);
    }

    private async Task<ParesMoneda> ObtenerParPorCodigosAsync(string origen, string destino)
    {
        return await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p =>
                p.MonedaOrigen.CodigoIso == origen &&
                p.MonedaDestino.CodigoIso == destino &&
                p.Activo)
            ?? throw new InvalidOperationException($"Par {origen}/{destino} no encontrado.");
    }
}
