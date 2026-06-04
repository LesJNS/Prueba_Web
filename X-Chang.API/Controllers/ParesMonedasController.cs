using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/pares")]
public class ParesMonedasController : ControllerBase
{
    private readonly IParesMonedasService _paresMonedasService;

    public ParesMonedasController(IParesMonedasService paresMonedasService)
    {
        _paresMonedasService = paresMonedasService;
    }

    private int? UsuarioIdOpcional =>
        User.Identity?.IsAuthenticated == true
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            : null;

    [HttpGet]
    public async Task<IActionResult> ObtenerPares([FromQuery] FiltroParesRequest filtro)
    {
        var result = await _paresMonedasService.ObtenerParesAsync(filtro, UsuarioIdOpcional);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerParDetalle(int id, [FromQuery] FiltroHistoricoRequest filtro)
    {
        try
        {
            var result = await _paresMonedasService.ObtenerParDetalleAsync(id, filtro);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("grafico-principal")]
    public async Task<IActionResult> ObtenerGraficoPrincipal()
    {
        try
        {
            var result = await _paresMonedasService.ObtenerGraficoPrincipalAsync(UsuarioIdOpcional);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
