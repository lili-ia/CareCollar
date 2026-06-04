using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using CareCollar.Server.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationController(INotificationService notificationService, IUserContext userContext) : ControllerBase
{
    /// <summary>Returns the 10 most recent notifications for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var notifications = await notificationService.GetLatestNotificationsForUserAsync(userId, ct);
        return Ok(notifications);
    }

    /// <summary>Returns all notifications for the authenticated user.</summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllNotifications(CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var notifications = await notificationService.GetAllNotificationsForUserAsync(userId, ct);
        return Ok(notifications);
    }

    /// <summary>Saves the FCM token for the authenticated user.</summary>
    [HttpPost("fcm-token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveFcmToken([FromBody] SaveFcmTokenRequest request, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await notificationService.SaveFcmTokenAsync(userId, request.Token, ct);
        return result.ToActionResult();
    }

    /// <summary>Marks a notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await notificationService.MarkAsReadAsync(id, userId, ct);
        return result.ToActionResult();
    }
}
