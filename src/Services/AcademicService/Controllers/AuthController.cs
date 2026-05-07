using Microsoft.AspNetCore.Mvc;
using Taller2NET.Shared.DTOs;
using AcademicService.Services;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Login and get JWT token</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result is null)
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", dto.Username);
            return Unauthorized(new { Success = false, Message = "Invalid credentials" });
        }
        _logger.LogInformation("User {Username} logged in", dto.Username);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>Register a new student account</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (result is null)
            return BadRequest(new { Success = false, Message = "Username or email already exists" });
        return CreatedAtAction(nameof(Login), new { Success = true, Data = result });
    }
}
