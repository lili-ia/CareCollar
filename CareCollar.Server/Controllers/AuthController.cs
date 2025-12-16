using CareCollar.Application.Contracts;
using CareCollar.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[ApiController]
[Route("api/auth")] // TODO: add versioning
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest req, CancellationToken ct)
    {
        var userResult = await authService.RegisterAsync(req.Email, req.Password, ct);
        
        if (!userResult.IsSuccess)
            return userResult.ToActionResult();
        
        var token = authService.GenerateJwtToken(userResult.Value!);
        
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest req, CancellationToken ct)
    {
        var userResult = await authService.ValidateUserAsync(req.Email, req.Password, ct);
        
        if (!userResult.IsSuccess)
            return userResult.ToActionResult();
        
        var token = authService.GenerateJwtToken(userResult.Value!);
        
        return Ok(token);
    }
}