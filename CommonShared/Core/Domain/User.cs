using CommonShared.Core.Domain.SharedKernel;

namespace CommonShared.Core.Domain;

public class User : Entity<long>
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    // public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    //public ICollection<UserChannel> Channels { get; set; } = new List<UserChannel>(); �� ����, ����� �� ����� �����, ������� ����� ����� ����� ���
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}