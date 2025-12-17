using System.ComponentModel.DataAnnotations;

namespace CommonShared.Core.Domain.DataTransferObjects;

public class NotificationResponseDTO
{
    public NotificationResponseDTO(Notification notification)
    {
        Type = notification.Type;
        Status = notification.Status;
        UserId = notification.UserId;
        CreatedAt = notification.CreatedAt;
        SentAt = notification.SentAt;
        ErrorMessage = notification.ErrorMessage;
        Id = notification.Id;
    }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public long Id { get; set; }
}