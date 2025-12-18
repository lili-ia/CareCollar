using CareCollar.Application.Contracts;
using CareCollar.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using CareCollar.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CareCollar.Controllers;

[ApiController]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController(IAuthService authService, IUserContext userContext) : ControllerBase
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
    
    /// <summary>
    /// Deletes the currently authenticated user's account.
    /// </summary>
    /// <param name="userContext">Service to access current user identity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="401">Unauthorized: Missing or invalid token.</response>
    /// <response code="404">User not found.</response>
    [Authorize] 
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromServices] IUserContext userContext, CancellationToken ct)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await authService.DeleteUserAsync(userId, ct);
        
        if (!result.IsSuccess)
            return result.ToActionResult();

        return NoContent();
    }
    
    /// <summary>
    /// [Admin Only] Retrieves a list of all registered users.
    /// </summary>
    /// <remarks>
    /// This is an **Administrative function**. 
    /// Access is restricted to accounts with the email: **admin@example.com**.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of user data transfer objects.</returns>
    /// <response code="200">Returns the list of all users.</response>
    /// <response code="401">Unauthorized: Missing or invalid JWT token.</response>
    /// <response code="403">Forbidden: Current user is not an administrator.</response>
    [Authorize]
    [HttpGet("admin/users")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var userId = userContext.UserId; 
        
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await authService.GetAllUsersAsync(userId, ct);
        
        return result.ToActionResult();
    }
}