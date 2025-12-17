using CommonShared.Core.Domain.SharedKernel;

namespace CommonShared.Core.Domain;

public class Notification : Entity<long>
{
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public ICollection<Media> Medias { get; set; } = new List<Media>();
}