using System.ComponentModel.DataAnnotations;

namespace CommonShared.Core.Domain.DataTransferObjects;

public class AllNotificationCreateDTO
{
    [Required]
    [MaxLength(500)]
    public String Body { get; set; } = null!;
    public List<IFormFile>? Files { get; set; }
}