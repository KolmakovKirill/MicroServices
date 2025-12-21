using CommonShared.Core.Domain;
using microservices_project.Configuration;
using Microsoft.Extensions.Options;

namespace microservices_project.Helpers;

public class NotificationHelper(IOptions<NotificationTopicsSettings> settings)
{
    private readonly NotificationTopicsSettings _notificationTopicsSettings = settings.Value;
    
    public string ResolveTopic(NotificationType notificationType)
    {
        return notificationType switch
        {
            NotificationType.SMS => _notificationTopicsSettings.Sms,
            NotificationType.Email => _notificationTopicsSettings.Email,
            NotificationType.Push => _notificationTopicsSettings.Push,
            NotificationType.Messenger => _notificationTopicsSettings.Messenger,
            _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
        };
    }
}