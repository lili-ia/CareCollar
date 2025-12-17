using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ThresholdController(IThresholdService thresholdService, IUserContext userContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ThresholdResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateThreshold([FromBody] CreateThresholdDto dto, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await thresholdService.CreateThresholdAsync(dto, userId, ct);
        return result.ToActionResult();
    }

    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ThresholdResponseDto>))]
    public async Task<IActionResult> GetPetThresholds(Guid petId, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await thresholdService.GetPetThresholdsAsync(petId, userId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteThreshold(Guid id, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await thresholdService.DeleteThresholdAsync(id, userId, ct);
        return result.ToActionResult();
    }
}