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
        var dtos = await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .Select(n => new NotificationDto
            {
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt
            })
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        return dtos;
    }
}