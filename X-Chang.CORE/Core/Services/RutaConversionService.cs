using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class RutaConversionService : IRutaConversionService
{
    private readonly ExchangeDivisasDbContext _context;

    public RutaConversionService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<BusquedaRutaDto> BuscarRutaAsync(int usuarioId, BuscarRutaRequest request)
    {
        if (request.Cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.");

        if (request.MaxSaltos < 1 || request.MaxSaltos > 5)
            throw new ArgumentException("MaxSaltos debe estar entre 1 y 5.");

        var par = await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p => p.ParMonedaId == request.ParMonedaId)
            ?? throw new InvalidOperationException("Par de moneda no encontrado.");

        var busqueda = new BusquedasRuta
        {
            UsuarioId = usuarioId,
            ParMonedaId = request.ParMonedaId,
            TipoOperacion = request.TipoOperacion,
            CantidadSolicitada = request.Cantidad,
            MaxSaltos = request.MaxSaltos,
            Estado = "Procesando",
            FechaInicio = DateTime.UtcNow
        };
        _context.BusquedasRuta.Add(busqueda);
        await _context.SaveChangesAsync();

        var inicio = DateTime.UtcNow;
        try
        {
            var paresActivos = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .Where(p => p.Activo)
                .ToListAsync();

            var rutas = EncontrarRutas(
                par.MonedaOrigenId,
                par.MonedaDestinoId,
                request.Cantidad,
                request.MaxSaltos,
                paresActivos);

            decimal? precioDirecto = ObtenerPrecioEstimado(paresActivos, par.MonedaOrigenId, par.MonedaDestinoId);
            decimal totalNormal = precioDirecto.HasValue ? request.Cantidad * precioDirecto.Value : 0;

            var rutasGuardadas = new List<RutasConversion>();
            foreach (var ruta in rutas)
            {
                var rutaDb = new RutasConversion
                {
                    BusquedaRutaId = busqueda.BusquedaRutaId,
                    MonedaInicialId = par.MonedaOrigenId,
                    MonedaFinalId = par.MonedaDestinoId,
                    CantidadSaltos = ruta.Count,
                    TotalEstimado = ruta.Last().ResultadoEstimado,
                    FechaCreacion = DateTime.UtcNow
                };

                if (totalNormal > 0 && rutaDb.TotalEstimado > totalNormal)
                    rutaDb.GananciaEstimada = rutaDb.TotalEstimado - totalNormal;
                else if (totalNormal > 0)
                    rutaDb.AhorroEstimado = totalNormal - rutaDb.TotalEstimado;

                _context.RutasConversion.Add(rutaDb);
                await _context.SaveChangesAsync();

                for (int i = 0; i < ruta.Count; i++)
                {
                    var salto = ruta[i];
                    _context.RutaConversionSaltos.Add(new RutaConversionSaltos
                    {
                        RutaConversionId = rutaDb.RutaConversionId,
                        NumeroSalto = i + 1,
                        ParMonedaId = salto.ParMonedaId,
                        MonedaOrigenId = salto.MonedaOrigenId,
                        MonedaDestinoId = salto.MonedaDestinoId,
                        CantidadConvertida = salto.CantidadEntrada,
                        ResultadoObtenido = salto.ResultadoEstimado,
                        PrecioPromedio = salto.PrecioEstimado
                    });
                }
                await _context.SaveChangesAsync();
                rutasGuardadas.Add(rutaDb);
            }

            var fin = DateTime.UtcNow;
            busqueda.Estado = "Completada";
            busqueda.FechaFin = fin;
            busqueda.TiempoRealMs = (int)(fin - inicio).TotalMilliseconds;
            if (totalNormal > 0) busqueda.TotalNormal = totalNormal;
            if (rutasGuardadas.Any())
            {
                var mejorRuta = rutasGuardadas.MaxBy(r => r.TotalEstimado);
                if (mejorRuta != null)
                {
                    busqueda.TotalRuta = mejorRuta.TotalEstimado;
                    busqueda.AhorroEstimado = mejorRuta.AhorroEstimado;
                    busqueda.GananciaEstimada = mejorRuta.GananciaEstimada;
                }
            }
            await _context.SaveChangesAsync();
        }
        catch
        {
            busqueda.Estado = "Fallida";
            busqueda.FechaFin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            throw;
        }

        return await ObtenerBusquedaConRutasAsync(busqueda.BusquedaRutaId, par);
    }

    public async Task<List<BusquedaRutaDto>> ObtenerMisBusquedasAsync(int usuarioId)
    {
        var busquedas = await _context.BusquedasRuta
            .Include(b => b.ParMoneda)
            .ThenInclude(p => p.MonedaOrigen)
            .Include(b => b.ParMoneda)
            .ThenInclude(p => p.MonedaDestino)
            .Where(b => b.UsuarioId == usuarioId)
            .OrderByDescending(b => b.FechaInicio)
            .Take(50)
            .ToListAsync();

        return busquedas.Select(b => new BusquedaRutaDto(
            b.BusquedaRutaId,
            b.ParMoneda.MonedaOrigen.CodigoIso,
            b.ParMoneda.MonedaDestino.CodigoIso,
            b.TipoOperacion, b.CantidadSolicitada, b.MaxSaltos,
            b.Estado, b.AhorroEstimado, b.GananciaEstimada,
            b.FechaInicio, null)).ToList();
    }

    private async Task<BusquedaRutaDto> ObtenerBusquedaConRutasAsync(int busquedaId, ParesMoneda par)
    {
        var busqueda = await _context.BusquedasRuta
            .Include(b => b.RutasConversion)
            .ThenInclude(r => r.RutaConversionSaltos)
            .ThenInclude(s => s.MonedaOrigen)
            .Include(b => b.RutasConversion)
            .ThenInclude(r => r.RutaConversionSaltos)
            .ThenInclude(s => s.MonedaDestino)
            .Include(b => b.RutasConversion)
            .ThenInclude(r => r.MonedaInicial)
            .Include(b => b.RutasConversion)
            .ThenInclude(r => r.MonedaFinal)
            .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaId)!;

        var rutasDto = busqueda!.RutasConversion.Select(r => new RutaConversionDto(
            r.RutaConversionId,
            r.MonedaInicial.CodigoIso,
            r.MonedaFinal.CodigoIso,
            r.CantidadSaltos,
            r.TotalEstimado,
            r.AhorroEstimado,
            r.GananciaEstimada,
            r.RutaConversionSaltos
                .OrderBy(s => s.NumeroSalto)
                .Select(s => new SaltoRutaDto(
                    s.NumeroSalto,
                    s.MonedaOrigen.CodigoIso,
                    s.MonedaDestino.CodigoIso,
                    s.MonedaOrigen.Nombre,
                    s.MonedaDestino.Nombre,
                    s.CantidadConvertida,
                    s.ResultadoObtenido,
                    s.PrecioPromedio))
                .ToList())).ToList();

        return new BusquedaRutaDto(
            busqueda.BusquedaRutaId,
            par.MonedaOrigen.CodigoIso,
            par.MonedaDestino.CodigoIso,
            busqueda.TipoOperacion,
            busqueda.CantidadSolicitada,
            busqueda.MaxSaltos,
            busqueda.Estado,
            busqueda.AhorroEstimado,
            busqueda.GananciaEstimada,
            busqueda.FechaInicio,
            rutasDto);
    }

    private record SaltoCalculo(int ParMonedaId, int MonedaOrigenId, int MonedaDestinoId,
        decimal CantidadEntrada, decimal ResultadoEstimado, decimal? PrecioEstimado);

    private List<List<SaltoCalculo>> EncontrarRutas(
        int monedaOrigenId, int monedaDestinoId,
        decimal cantidad, int maxSaltos,
        List<ParesMoneda> pares)
    {
        var resultado = new List<List<SaltoCalculo>>();
        var cola = new Queue<(int monedaActual, decimal cantidadActual, List<SaltoCalculo> camino, HashSet<int> visitados)>();
        cola.Enqueue((monedaOrigenId, cantidad, new List<SaltoCalculo>(), new HashSet<int> { monedaOrigenId }));

        while (cola.Count > 0)
        {
            var (monedaActual, cantActual, camino, visitados) = cola.Dequeue();

            if (camino.Count > maxSaltos) continue;

            var conexiones = pares.Where(p => p.MonedaOrigenId == monedaActual).ToList();

            foreach (var par in conexiones)
            {
                if (visitados.Contains(par.MonedaDestinoId) && par.MonedaDestinoId != monedaDestinoId)
                    continue;

                var precioEstimado = ObtenerPrecioEstimado(pares, par.MonedaOrigenId, par.MonedaDestinoId);
                if (!precioEstimado.HasValue) continue;

                var resultado_salto = cantActual * precioEstimado.Value;

                var nuevoCamino = new List<SaltoCalculo>(camino)
                {
                    new(par.ParMonedaId, par.MonedaOrigenId, par.MonedaDestinoId,
                        cantActual, resultado_salto, precioEstimado)
                };

                if (par.MonedaDestinoId == monedaDestinoId)
                {
                    resultado.Add(nuevoCamino);
                }
                else if (nuevoCamino.Count < maxSaltos)
                {
                    var nuevosVisitados = new HashSet<int>(visitados) { par.MonedaDestinoId };
                    cola.Enqueue((par.MonedaDestinoId, resultado_salto, nuevoCamino, nuevosVisitados));
                }
            }
        }

        return resultado.OrderByDescending(r => r.Last().ResultadoEstimado).Take(5).ToList();
    }

    private static decimal? ObtenerPrecioEstimado(List<ParesMoneda> pares, int monedaOrigenId, int monedaDestinoId)
    {
        var par = pares.FirstOrDefault(p =>
            p.MonedaOrigenId == monedaOrigenId && p.MonedaDestinoId == monedaDestinoId);
        return par != null ? (decimal?)1.0m : null;
    }
}
