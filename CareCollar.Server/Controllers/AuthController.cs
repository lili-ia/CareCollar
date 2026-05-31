using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CareCollar.Controllers;

[ApiController]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController(IAuthService authService, IUserContext userContext) : ControllerBase
{
    /// <summary>Registers a new user and returns a JWT token.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest req, CancellationToken ct)
    {
        var userResult = await authService.RegisterAsync(req.Email, req.Password, ct);

        if (!userResult.IsSuccess)
            return userResult.ToActionResult();

        return Ok(authService.GenerateTokenResponse(userResult.Value!));
    }

    /// <summary>Authenticates a user and returns a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest req, CancellationToken ct)
    {
        var userResult = await authService.ValidateUserAsync(req.Email, req.Password, ct);

        if (!userResult.IsSuccess)
            return userResult.ToActionResult();

        return Ok(authService.GenerateTokenResponse(userResult.Value!));
    }

    /// <summary>Returns the current authenticated user's profile.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.GetCurrentUserAsync(userId, ct);
        return result.ToActionResult();
    }

    /// <summary>Deletes the currently authenticated user's account.</summary>
    [Authorize]
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.DeleteUserAsync(userId, ct);
        if (!result.IsSuccess) return result.ToActionResult();

        return NoContent();
    }

    /// <summary>[Admin] Returns all registered users.</summary>
    [Authorize]
    [HttpGet("admin/users")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.GetAllUsersAsync(userId, ct);
        return result.ToActionResult();
    }

    /// <summary>[Admin] Grants or revokes admin status for a user.</summary>
    [Authorize]
    [HttpPatch("admin/users/{targetId:guid}/admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAdminStatus(Guid targetId, [FromBody] SetAdminStatusRequest req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.SetAdminStatusAsync(userId, targetId, req.IsAdmin, ct);
        return result.ToActionResult();
    }

    /// <summary>[Admin] Deletes a user by ID.</summary>
    [Authorize]
    [HttpDelete("admin/users/{targetId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdminDeleteUser(Guid targetId, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.AdminDeleteUserAsync(userId, targetId, ct);
        return result.ToActionResult();
    }
}