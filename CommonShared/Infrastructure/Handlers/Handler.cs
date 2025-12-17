using CommonShared.Core.Domain;

namespace CommonShared.Infrastructure.Handlers;

public abstract class Handler
{
    public NotificationType Type { get; set; }
    public abstract void HandleTask(Notification notification, CancellationToken cancellationToken); // Хз, тут напишите, как вам будет удобно
}