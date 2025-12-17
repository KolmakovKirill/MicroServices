using System.ComponentModel.DataAnnotations;

namespace CommonShared.Core.Domain.DataTransferObjects;

public class CreateUserDTO
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Phone]
    public string? PhoneNumber { get; set; }

    // [Required]
    // [MinLength(6)]
    // public string Password { get; set; } = null!;
}   