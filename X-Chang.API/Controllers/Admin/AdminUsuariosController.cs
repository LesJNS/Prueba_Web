using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers.Admin;

[ApiController]
[Route("api/admin/usuarios")]
[Authorize(Roles = "Admin")]
public class AdminUsuariosController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminUsuariosController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private int AdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ObtenerUsuarios([FromQuery] FiltroAdminRequest filtro)
    {
        var result = await _adminService.ObtenerUsuariosAsync(filtro);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerUsuario(int id)
    {
        try
        {
            var result = await _adminService.ObtenerUsuarioAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoUsuarioRequest request)
    {
        if (id != request.UsuarioId)
            return BadRequest(new { error = "El ID en la ruta no coincide con el del cuerpo." });

        try
        {
            await _adminService.CambiarEstadoUsuarioAsync(AdminId, request);
            return NoContent();
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

    [HttpPost("{id:int}/restricciones")]
    public async Task<IActionResult> AplicarRestriccion(int id, [FromBody] RestriccionRequest request)
    {
        if (id != request.UsuarioId)
            return BadRequest(new { error = "El ID en la ruta no coincide con el del cuerpo." });

        try
        {
            var result = await _adminService.AplicarRestriccionAsync(AdminId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:int}/restringir")]
    public async Task<IActionResult> RestringirUsuario(int id, [FromBody] RestringirUsuarioRequest request)
    {
        if (id != request.UsuarioId)
            return BadRequest(new { error = "El ID en la ruta no coincide con el del cuerpo." });
        try
        {
            await _adminService.RestringirUsuarioAsync(AdminId, request);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:int}/habilitar")]
    public async Task<IActionResult> HabilitarUsuario(int id, [FromBody] HabilitarUsuarioRequest request)
    {
        if (id != request.UsuarioId)
            return BadRequest(new { error = "El ID en la ruta no coincide con el del cuerpo." });
        try
        {
            await _adminService.HabilitarUsuarioAsync(AdminId, request);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("auditoria")]
    public async Task<IActionResult> ObtenerAuditoria([FromQuery] FiltroAuditoriaRequest filtro)
    {
        var result = await _adminService.ObtenerAuditoriaAsync(filtro);
        return Ok(result);
    }
}
