using CommonShared.Core.Domain;

namespace CommonShared.Infrastructure.Handlers;

public abstract class NotificationHandler
{
    public NotificationType Type { get; set; }
    public abstract Task HandleTask(Notification notification, CancellationToken cancellationToken); // Хз, тут напишите, как вам будет удобно
}