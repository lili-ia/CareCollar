namespace CareCollar.Application.Contracts;

public interface IFcmService
{
    Task SendAsync(string fcmToken, string title, string body, CancellationToken ct = default);
}
