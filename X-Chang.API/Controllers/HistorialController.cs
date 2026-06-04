using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/historial")]
[Authorize]
public class HistorialController : ControllerBase
{
    private readonly IHistorialService _historialService;

    public HistorialController(IHistorialService historialService)
    {
        _historialService = historialService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ObtenerHistorial([FromQuery] FiltroHistorialRequest filtro)
    {
        var result = await _historialService.ObtenerHistorialAsync(UsuarioId, filtro);
        return Ok(result);
    }

    [HttpGet("completo")]
    public async Task<IActionResult> ObtenerHistorialCompleto(
        [FromQuery] DateTime? ordenesDesde = null, [FromQuery] DateTime? ordenesHasta = null, [FromQuery] int ordenesPagina = 1, [FromQuery] int ordenesTamano = 10,
        [FromQuery] DateTime? ofertasDesde = null, [FromQuery] DateTime? ofertasHasta = null, [FromQuery] int ofertasPagina = 1, [FromQuery] int ofertasTamano = 10,
        [FromQuery] DateTime? comprasDesde = null, [FromQuery] DateTime? comprasHasta = null, [FromQuery] int comprasPagina = 1, [FromQuery] int comprasTamano = 10,
        [FromQuery] DateTime? ventasDesde = null, [FromQuery] DateTime? ventasHasta = null, [FromQuery] int ventasPagina = 1, [FromQuery] int ventasTamano = 10,
        [FromQuery] DateTime? depositosDesde = null, [FromQuery] DateTime? depositosHasta = null, [FromQuery] int depositosPagina = 1, [FromQuery] int depositosTamano = 10,
        [FromQuery] DateTime? retirosDesde = null, [FromQuery] DateTime? retirosHasta = null, [FromQuery] int retirosPagina = 1, [FromQuery] int retirosTamano = 10)
    {
        var result = await _historialService.ObtenerHistorialCompletoAsync(
            UsuarioId,
            new FiltroColumnaRequest(ordenesDesde, ordenesHasta, ordenesPagina, ordenesTamano),
            new FiltroColumnaRequest(ofertasDesde, ofertasHasta, ofertasPagina, ofertasTamano),
            new FiltroColumnaRequest(comprasDesde, comprasHasta, comprasPagina, comprasTamano),
            new FiltroColumnaRequest(ventasDesde, ventasHasta, ventasPagina, ventasTamano),
            new FiltroColumnaRequest(depositosDesde, depositosHasta, depositosPagina, depositosTamano),
            new FiltroColumnaRequest(retirosDesde, retirosHasta, retirosPagina, retirosTamano));
        return Ok(result);
    }
}
