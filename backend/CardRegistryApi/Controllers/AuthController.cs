using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CardRegistryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CadastroDto dto)
    {
        var response = await _service.Register(dto);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var response = await _service.Login(dto);
        if (!response.Status)
            return Unauthorized(response);

        return Ok(response);
    }
}
