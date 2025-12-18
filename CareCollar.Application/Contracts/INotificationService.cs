using CareCollar.Application.DTOs;
using CareCollar.Shared;

namespace CareCollar.Application.Contracts;

public interface INotificationService
{
    Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct);
    
    Task<List<NotificationDto>> GetLatestNotificationsForUserAsync(Guid userId, CancellationToken ct);
}