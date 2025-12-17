using CommonShared.Core.Domain;

namespace CommonShared.Core.Domain.Channels;

public interface INotificationChannel
{
    NotificationType ChannelType { get; }
    Task<ChannelSendResult> SendAsync(Notification notification, string recipient, List<string>? mediaUrls = null);
}

public class ChannelSendResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
}
