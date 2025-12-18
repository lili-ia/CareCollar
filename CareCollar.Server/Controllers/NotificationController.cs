using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController(INotificationService notificationService, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Retrieves the 10 most recent notifications for the authenticated user.
    /// </summary>
    /// <param name="ct">A cancellation token to abort the request.</param>
    /// <returns>A list of notification objects belonging to the user.</returns>
    /// <response code="200">Successfully retrieved the list of notifications.</response>
    /// <response code="401">User is not authenticated or the token is invalid.</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(CancellationToken ct = default)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        var notifications = await notificationService.GetLatestNotificationsForUserAsync(userId, ct);
        return Ok(notifications);
    }
}