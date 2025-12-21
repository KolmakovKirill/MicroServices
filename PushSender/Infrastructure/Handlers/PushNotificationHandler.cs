using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using FirebaseAdmin.Messaging;
using PushSender.Services;
using Notification = CommonShared.Core.Domain.Notification;

namespace PushSender.Infrastructure.Handlers;

public class PushNotificationHandler : NotificationHandler
{
    private readonly NotificationService _notificationService;
    private readonly IFirebaseAppProvider _firebaseAppProvider;
    private readonly ILogger<PushNotificationHandler> _logger;

    public PushNotificationHandler(
        NotificationService notificationService,
        IFirebaseAppProvider firebaseAppProvider,
        ILogger<PushNotificationHandler> logger)
    {
        Type = NotificationType.Push;
        _notificationService = notificationService;
        _firebaseAppProvider = firebaseAppProvider;
        _logger = logger;
    }

    public override async Task HandleTask(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting push notification processing for notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Processing;
            await _notificationService.UpdateAsync(notification);

            var deviceToken = notification.User.DeviceToken;
            if (string.IsNullOrWhiteSpace(deviceToken))
                throw new InvalidOperationException("Device token is not specified for push notifications");

            await SendPushNotificationAsync(notification, deviceToken, cancellationToken);

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _notificationService.UpdateAsync(notification);

            _logger.LogInformation("Push notification {NotificationId} sent successfully", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            await _notificationService.UpdateAsync(notification);
        }
    }

    private async Task SendPushNotificationAsync(Notification notification, string deviceToken, CancellationToken cancellationToken)
    {
        var app = _firebaseAppProvider.TryGetApp();
        if (app == null)
        {
            await SendMockPushNotificationAsync(notification, deviceToken, cancellationToken);
            return;
        }

        var messaging = FirebaseMessaging.GetMessaging(app);

        var data = new Dictionary<string, string>
        {
            ["notificationId"] = notification.Id.ToString(),
            ["type"] = notification.Type.ToString()
        };

        if (notification.Medias.Any())
        {
            data["hasAttachments"] = "true";
            data["attachmentCount"] = notification.Medias.Count.ToString();
        }

        var message = new Message
        {
            Token = deviceToken,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = notification.Subject ?? "Notification",
                Body = notification.Body
            },
            Data = data
        };

        var result = await messaging.SendAsync(message, cancellationToken);
        _logger.LogInformation("FCM message sent successfully. Message ID: {MessageId}", result);
    }

    private async Task SendMockPushNotificationAsync(Notification notification, string deviceToken, CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken);

        if (Random.Shared.Next(100) < 5)
            throw new Exception("Simulated push notification sending failure");

        _logger.LogInformation("Mock push notification sent successfully. NotificationId={NotificationId}", notification.Id);
    }
}