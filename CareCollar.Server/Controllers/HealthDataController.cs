using CareCollar.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/health")]
public class HealthDataController(
    IHealthDataRepository dataRepo, 
    IUserContext userContext, 
    ICollarService collarService) : ControllerBase
{
    [HttpGet("history/{collarId:Guid}")]
    public async Task<IActionResult> GetHistory(
        Guid collarId, 
        [FromQuery] DateTime from, 
        [FromQuery] DateTime to,
        [FromQuery] int bucketMinutes = 5,
        CancellationToken ct = default) 
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var owns = await collarService.UserOwnsCollarAsync(collarId, userId, ct);
        if (!owns)
        {
            return NotFound("Collar not found.");
        }
        
        var interval = TimeSpan.FromMinutes(bucketMinutes);
        var history = await dataRepo.GetHistoryAsync(collarId, from, to, interval);
        return Ok(history);
    }
}