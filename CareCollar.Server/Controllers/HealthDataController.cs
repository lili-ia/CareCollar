using System.Net.Mime;
using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/health")]
[Produces(MediaTypeNames.Application.Json)]
public class HealthDataController(
    IHealthDataRepository dataRepo, 
    IUserContext userContext, 
    ICollarService collarService,
    IIngestionService ingestionService) : ControllerBase
{
    /// <summary>
    /// Retrieves historical health data for a specific collar.
    /// </summary>
    /// <param name="collarId">The internal Guid of the collar.</param>
    /// <param name="from">Start timeframe.</param>
    /// <param name="to">End timeframe.</param>
    /// <param name="bucketMinutes">Time aggregation interval (for TimescaleDB hypertable).</param>
    [HttpGet("history/{collarId:Guid}")]
    [ProducesResponseType(typeof(IEnumerable<HealthHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        Guid collarId, 
        [FromQuery] DateTime from, 
        [FromQuery] DateTime to,
        CancellationToken ct,
        [FromQuery] int bucketMinutes = 5) 
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var owns = await collarService.UserOwnsCollarAsync(collarId, userId, ct);
        if (!owns) return NotFound("Collar not found.");
        
        var interval = TimeSpan.FromMinutes(bucketMinutes);
        var history = await dataRepo.GetHistoryAsync(collarId, from, to, interval);
        return Ok(history);
    }
    
    /// <summary>
    /// Receives real-time telemetry from the smart collar hardware.
    /// </summary>
    /// <remarks>
    /// This endpoint is called by the ESP32. It maps the serial number to a database ID 
    /// and performs business validation.
    /// </remarks>
    [HttpPost("ingest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IngestData([FromBody] HealthDataIngestionDto data, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        var result = await ingestionService.ProcessDataAsync(data, userId, ct);
        return result.ToActionResult();
    }
}