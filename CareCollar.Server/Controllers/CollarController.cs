using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CareCollar.Controllers;

[ApiController]
[Route("api/collars")]
[Produces(MediaTypeNames.Application.Json)]
public class CollarController(ICollarService collarService, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Binds a physical collar to a specific pet.
    /// </summary>
    /// <remarks>
    /// This is a **User function**. It allows the pet owner to link a collar they purchased to their pet's profile.
    /// </remarks>
    /// <response code="200">Collar successfully bound to the pet.</response>
    /// <response code="401">User is not authorized and can not access this resource.</response>
    /// <response code="404">Pet or Collar not found.</response>
    /// <response code="409">Collar is already assigned to another pet.</response>
    [Authorize]
    [HttpPost("bind")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BindCollar([FromBody] BindCollarDto dto, CancellationToken ct)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await collarService.BindToPetAsync(dto, userId, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Registers a new collar serial number in the system.
    /// </summary>
    /// <remarks>
    /// This is an **Administration function**. Usually performed by a system administrator or factory worker 
    /// to add new hardware to the global database.
    /// </remarks>
    /// <response code="200">Device registered successfully.</response>
    /// <response code="401">User is not authorized and can not access this resource.</response>
    /// <response code="403">Forbidden: Current user is not an administrator.</response>
    /// <response code="500">Internal server error during registration.</response>
    [HttpPost("admin/register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterCollarDto dto, CancellationToken ct)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        var result = await collarService.RegisterDeviceAsync(dto, userId, ct);
        return result.ToActionResult();
    }
}