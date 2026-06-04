using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminReportesController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IHistorialService _historialService;

    public AdminReportesController(IAdminService adminService, IHistorialService historialService)
    {
        _adminService = adminService;
        _historialService = historialService;
    }

    private int AdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("configuracion")]
    public async Task<IActionResult> ObtenerConfiguraciones()
    {
        var result = await _adminService.ObtenerConfiguracionesAsync();
        return Ok(result);
    }

    [HttpPut("configuracion/{id:int}")]
    public async Task<IActionResult> ActualizarConfiguracion(
        int id, [FromBody] ActualizarConfiguracionRequest request)
    {
        try
        {
            var result = await _adminService.ActualizarConfiguracionAsync(AdminId, id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("reportes/historial")]
    public async Task<IActionResult> ReporteHistorial(
        [FromQuery] int usuarioId,
        [FromQuery] FiltroHistorialRequest filtro)
    {
        var result = await _historialService.ObtenerHistorialAsync(usuarioId, filtro);
        return Ok(result);
    }

    [HttpGet("reportes/pares")]
    public async Task<IActionResult> ReportePares([FromQuery] FiltroDashboardRequest filtro)
    {
        var dashboard = await _adminService.ObtenerDashboardAsync(filtro);
        return Ok(new { TopPares = dashboard.TopPares });
    }
}
