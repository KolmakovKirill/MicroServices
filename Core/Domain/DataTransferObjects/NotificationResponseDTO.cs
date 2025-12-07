using System.ComponentModel.DataAnnotations;

namespace microservices_project.Core.Domain.DataTransferObjects;

public class NotificationResponseDTO
{
    public NotificationResponseDTO(Notification notification)
    {
        Subject = notification.Subject;
        Body = notification.Body;
        Type = notification.Type;
        Status = notification.Status;
        UserId = notification.UserId;
        CreatedAt = notification.CreatedAt;
        SentAt = notification.SentAt;
        ErrorMessage = notification.ErrorMessage;
        Medias = notification.Medias.Select(media => new MediaResponseDTO(media)).ToList();
    }

    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public ICollection<MediaResponseDTO> Medias { get; set; } = new List<MediaResponseDTO>();
}