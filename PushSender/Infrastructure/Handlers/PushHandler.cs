using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using NotificationDomain = CommonShared.Core.Domain.Notification;

public class PushHandler : Handler
{
    private readonly NotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PushHandler> _logger;
    private FirebaseApp? _firebaseApp;

    public PushHandler(NotificationService notificationService, IConfiguration configuration, ILogger<PushHandler> logger)
    {
        Type = NotificationType.Push;
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;

        InitializeFirebase();
    }

    public override async void HandleTask(NotificationDomain notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting push notification processing for notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Processing;
            await _notificationService.UpdateAsync(notification);

            var deviceToken = notification.User.DeviceToken;
            if (string.IsNullOrEmpty(deviceToken))
            {
                throw new InvalidOperationException("Device token is not specified for push notifications");
            }

            await SendPushNotificationAsync(notification, deviceToken, cancellationToken);

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _notificationService.UpdateAsync(notification);

            _logger.LogInformation("Push notification {NotificationId} sent successfully to device {DeviceToken}", notification.Id, deviceToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            await _notificationService.UpdateAsync(notification);
        }
    }

    private void InitializeFirebase()
    {
        try
        {
            var firebaseSettings = _configuration.GetSection("FirebaseSettings");
            var credentialsPath = firebaseSettings["CredentialsPath"];

            if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(credentialsPath)
                }, "PushSender");
            }
            else
            {
                _logger.LogWarning("Firebase credentials not configured, using mock mode");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase");
        }
    }

    private async Task SendPushNotificationAsync(NotificationDomain notification, string deviceToken, CancellationToken cancellationToken)
    {
        if (_firebaseApp == null)
        {
            await SendMockPushNotificationAsync(notification, deviceToken, cancellationToken);
            return;
        }

        var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);

        var data = new Dictionary<string, string>()
        {
            ["notificationId"] = notification.Id.ToString(),
            ["type"] = notification.Type.ToString()
        };

        if (notification.Medias != null && notification.Medias.Any())
        {
            data["hasAttachments"] = "true";
            data["attachmentCount"] = notification.Medias.Count.ToString();
        }

        var message = new FirebaseAdmin.Messaging.Message()
        {
            Token = deviceToken,
            Notification = new FirebaseAdmin.Messaging.Notification()
            {
                Title = notification.Subject ?? "Notification",
                Body = notification.Body
            },
            Data = data
        };

        var result = await messaging.SendAsync(message, cancellationToken);
        _logger.LogInformation("FCM message sent successfully. Message ID: {MessageId}", result);
    }

    private async Task SendMockPushNotificationAsync(NotificationDomain notification, string deviceToken, CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken);

        if (new Random().Next(100) < 5) 
        {
            throw new Exception("Simulated push notification sending failure");
        }

        _logger.LogInformation("Mock push notification sent successfully to device {DeviceToken} with title: {Title}, body: {Body}",
            deviceToken, notification.Subject, notification.Body);
    }
}
