using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/operaciones")]
[Authorize]
public class OperacionController : ControllerBase
{
    private readonly IOperacionInmediataService _operacionService;

    public OperacionController(IOperacionInmediataService operacionService)
    {
        _operacionService = operacionService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> EjecutarOperacion([FromBody] OperacionInmediataRequest request)
    {
        try
        {
            var result = await _operacionService.EjecutarOperacionAsync(UsuarioId, request);
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

    [HttpGet]
    public async Task<IActionResult> ObtenerMisOperaciones()
    {
        var result = await _operacionService.ObtenerMisOperacionesAsync(UsuarioId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerOperacion(int id)
    {
        try
        {
            var result = await _operacionService.ObtenerOperacionAsync(UsuarioId, id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
