using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Domain.Entities;
using CareCollar.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareCollar.Application.Services;

public class NotificationService(ICareCollarDbContext context, ILogger<NotificationService> logger)
    : INotificationService
{
    public async Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message
        };

        await context.Notifications.AddAsync(notification, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Notification stored in DB for User {UserId}: {Title}", userId, title);
    }

    public async Task<List<NotificationDto>> GetLatestNotificationsForUserAsync(Guid userId, CancellationToken ct)
    {
        return await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync(ct);
    }

    public async Task<List<NotificationDto>> GetAllNotificationsForUserAsync(Guid userId, CancellationToken ct)
    {
        return await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync(ct);
    }

    public async Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification is null)
            return Result.Failure("Notification not found", ErrorType.NotFound);

        notification.IsRead = true;
        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
