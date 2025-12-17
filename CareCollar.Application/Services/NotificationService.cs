using CareCollar.Application.Contracts;
using CareCollar.Domain.Entities;
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
}