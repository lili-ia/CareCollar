using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/collars")] // TODO: add docs
public class CollarController(ICollarService collarService, IUserContext userContext) : ControllerBase
{
    [HttpPost("bind")]
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCollarDto dto, CancellationToken ct)
    {
        var result = await collarService.RegisterDeviceAsync(dto, ct);
        return result.ToActionResult();
    }
}