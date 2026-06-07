using CareCollar.Application.Contracts;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace CareCollar.Infrastructure.Firebase;

public class FcmService(ILogger<FcmService> logger) : IFcmService
{
    public async Task SendAsync(string fcmToken, string title, string body, CancellationToken ct = default)
    {
        var message = new Message
        {
            Token = fcmToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High
            }
        };

        try
        {
            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
            logger.LogInformation("FCM push sent. MessageId: {MessageId}", messageId);
        }
        catch (FirebaseMessagingException ex) when (
            ex.MessagingErrorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
        {
            logger.LogWarning("FCM token is invalid or unregistered. Token: {Token}, Error: {Error}",
                fcmToken[..Math.Min(20, fcmToken.Length)], ex.Message);
        }
    }
}
