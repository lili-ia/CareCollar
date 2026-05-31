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
    /// <summary>Returns all collars bound to a specific pet.</summary>
    [Authorize]
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(List<CollarDeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPetCollars(Guid petId, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await collarService.GetPetCollarsAsync(petId, userId, ct);
        return result.ToActionResult();
    }

    /// <summary>Binds a collar to a pet using the serial number printed on the device.</summary>
    [Authorize]
    [HttpPost("bind-by-serial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BindCollarBySerial([FromBody] BindCollarBySerialDto dto, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await collarService.BindToPetBySerialAsync(dto, userId, ct);
        return result.ToActionResult();
    }

    /// <summary>Binds a physical collar to a specific pet by internal ID.</summary>
    [Authorize]
    [HttpPost("bind")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BindCollar([FromBody] BindCollarDto dto, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await collarService.BindToPetAsync(dto, userId, ct);
        return result.ToActionResult();
    }

    /// <summary>[Admin] Registers a new collar serial number.</summary>
    [Authorize]
    [HttpPost("admin/register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterCollarDto dto, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await collarService.RegisterDeviceAsync(dto, userId, ct);
        return result.ToActionResult();
    }

    /// <summary>[Admin] Returns all registered collars in the system.</summary>
    [Authorize]
    [HttpGet("admin")]
    [ProducesResponseType(typeof(List<CollarDeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllCollars(CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await collarService.GetAllCollarsAsync(userId, ct);
        return result.ToActionResult();
    }
}
