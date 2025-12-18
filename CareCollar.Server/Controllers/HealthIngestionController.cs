using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[ApiController]
[Route("api/health")]
[Authorize] 
public class HealthIngestionController(IIngestionService ingestionService, IUserContext userContext) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<IActionResult> IngestData([FromBody] HealthDataIngestionDto data, CancellationToken ct = default)
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