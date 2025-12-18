using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/thresholds")] // Changed to plural for REST standards
[Produces(MediaTypeNames.Application.Json)]
public class ThresholdController(IThresholdService thresholdService, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Creates a health threshold for a pet (e.g., Min/Max Heart Rate or Temperature).
    /// </summary>
    /// <remarks>
    /// This defines the "Safety Zone" for a pet. If incoming telemetry falls outside these values, 
    /// the business logic will trigger an alert.
    /// </remarks>
    /// <response code="200">Threshold created successfully.</response>
    /// <response code="401">Unauthorized: User context missing.</response>
    /// <response code="404">Pet not found or does not belong to the user.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ThresholdResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateThreshold([FromBody] CreateThresholdDto dto, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await thresholdService.CreateThresholdAsync(dto, userId, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves all active thresholds for a specific pet.
    /// </summary>
    /// <param name="petId">The Guid of the pet.</param>
    /// <response code="200">List of thresholds returned.</response>
    /// <response code="401">Unauthorized: User context missing.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ThresholdResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPetThresholds(Guid petId, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await thresholdService.GetPetThresholdsAsync(petId, userId, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a specific threshold by its ID.
    /// </summary>
    /// <param name="id">The Guid of the threshold.</param>
    /// <response code="204">No Content (Successful deletion).</response>
    /// <response code="401">Unauthorized: User context missing.</response>
    /// <response code="404">Threshold not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteThreshold(Guid id, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await thresholdService.DeleteThresholdAsync(id, userId, ct);
        return result.ToActionResult();
    }
}