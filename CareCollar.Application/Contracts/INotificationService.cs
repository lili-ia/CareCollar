namespace CareCollar.Application.Contracts;

public interface INotificationService
{
    Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct);
}