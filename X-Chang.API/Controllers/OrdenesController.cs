using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;

    public OrdenesController(IOrdenService ordenService)
    {
        _ordenService = ordenService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest request)
    {
        try
        {
            var result = await _ordenService.CrearOrdenCompraAsync(UsuarioId, request);
            return CreatedAtAction(nameof(ObtenerOrden), new { id = result.OrdenCompraId }, result);
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
    public async Task<IActionResult> ObtenerMisOrdenes()
    {
        var result = await _ordenService.ObtenerMisOrdenesAsync(UsuarioId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerOrden(int id)
    {
        try
        {
            var result = await _ordenService.ObtenerOrdenAsync(UsuarioId, id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelarOrden(int id)
    {
        try
        {
            await _ordenService.CancelarOrdenAsync(UsuarioId, id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("libro/{parMonedaId:int}")]
    public async Task<IActionResult> ObtenerLibroOrdenes(int parMonedaId)
    {
        var result = await _ordenService.ObtenerLibroOrdenesAsync(parMonedaId);
        return Ok(result);
    }
}
