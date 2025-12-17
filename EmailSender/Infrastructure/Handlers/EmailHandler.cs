using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;

public class EmailHandler : Handler
{
    private readonly NotificationService _notificationService;

    public EmailHandler(NotificationService notificationService)
    {
        Type = NotificationType.Email;
        _notificationService = notificationService;
    }

    public override void HandleTask(Notification notification, CancellationToken cancellationToken)
    {
        // _notificationService.
        Console.WriteLine("Im currently handling this notification");
    }
}