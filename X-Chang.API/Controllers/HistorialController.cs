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
}
