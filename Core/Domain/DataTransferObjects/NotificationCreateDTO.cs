using System.ComponentModel.DataAnnotations;

namespace microservices_project.Core.Domain.DataTransferObjects;

public class NotificationCreateDTO
{
    [Required]
    [MaxLength(500)]
    public String Body { get; set; } = null!;
    [Required]
    public NotificationType Type { get; set; } 
    public List<IFormFile> Files { get; set; } = new List<IFormFile>();
}