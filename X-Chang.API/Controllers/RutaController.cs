using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/rutas")]
[Authorize]
public class RutaController : ControllerBase
{
    private readonly IRutaConversionService _rutaService;

    public RutaController(IRutaConversionService rutaService)
    {
        _rutaService = rutaService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("buscar")]
    public async Task<IActionResult> BuscarRuta([FromBody] BuscarRutaRequest request)
    {
        try
        {
            var result = await _rutaService.BuscarRutaAsync(UsuarioId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("mis-busquedas")]
    public async Task<IActionResult> ObtenerMisBusquedas()
    {
        var result = await _rutaService.ObtenerMisBusquedasAsync(UsuarioId);
        return Ok(result);
    }
}
