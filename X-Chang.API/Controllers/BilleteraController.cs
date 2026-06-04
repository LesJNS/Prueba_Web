using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/billetera")]
[Authorize]
public class BilleteraController : ControllerBase
{
    private readonly IBilleteraService _billeteraService;

    public BilleteraController(IBilleteraService billeteraService)
    {
        _billeteraService = billeteraService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ObtenerBilletera()
    {
        try
        {
            var result = await _billeteraService.ObtenerBilleteraAsync(UsuarioId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("depositar")]
    public async Task<IActionResult> Depositar([FromBody] DepositoRequest request)
    {
        try
        {
            var result = await _billeteraService.DepositarAsync(UsuarioId, request);
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

    [HttpPost("retirar")]
    public async Task<IActionResult> Retirar([FromBody] RetiroRequest request)
    {
        try
        {
            var result = await _billeteraService.RetirarAsync(UsuarioId, request);
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

    [HttpGet("movimientos")]
    public async Task<IActionResult> ObtenerMovimientos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20)
    {
        var result = await _billeteraService.ObtenerMovimientosAsync(UsuarioId, pagina, tamano);
        return Ok(result);
    }

    [HttpGet("depositos")]
    public async Task<IActionResult> ObtenerDepositos()
    {
        var result = await _billeteraService.ObtenerDepositosAsync(UsuarioId);
        return Ok(result);
    }

    [HttpGet("retiros")]
    public async Task<IActionResult> ObtenerRetiros()
    {
        var result = await _billeteraService.ObtenerRetirosAsync(UsuarioId);
        return Ok(result);
    }

    [HttpGet("metodos-pago")]
    public async Task<IActionResult> ObtenerMetodosPago()
    {
        try
        {
            var result = await _billeteraService.ObtenerMetodosPagoAsync(UsuarioId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
