using CareCollar.Application.Contracts;
using CareCollar.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CareCollar.Controllers;

[ApiController]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="req">The registration request containing email and password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A JWT token upon successful registration.</returns>
    /// <response code="200">User created successfully and token returned.</response>
    /// <response code="409">Conflict: A user with this email already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest req, CancellationToken ct)
    {
        var userResult = await authService.RegisterAsync(req.Email, req.Password, ct);
        
        if (!userResult.IsSuccess)
            return userResult.ToActionResult();
        
        var token = authService.GenerateJwtToken(userResult.Value!);
        
        return Ok(token);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="req">The login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A JWT token if credentials are valid.</returns>
    /// <response code="200">Authentication successful.</response>
    /// <response code="401">Unauthorized: Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest req, CancellationToken ct)
    {
        var userResult = await authService.ValidateUserAsync(req.Email, req.Password, ct);
        
        if (!userResult.IsSuccess)
            return userResult.ToActionResult();
        
        var token = authService.GenerateJwtToken(userResult.Value!);
        
        return Ok(token);
    }
}