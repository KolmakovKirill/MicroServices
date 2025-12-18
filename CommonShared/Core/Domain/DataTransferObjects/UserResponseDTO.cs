namespace CommonShared.Core.Domain.DataTransferObjects;

public class UserResponseDTO
{
    public UserResponseDTO(User user)
    {
        Username = user.Username;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        Id = user.Id;
        MessengerId = user.MessengerId;
        DeviceToken = user.DeviceToken;
    }

    public long Id { get; set; }
    public string Username { get; set; }
    public string Email {get; set;}
    public string PhoneNumber { get; set; }
    public string MessengerId { get; set; }
    public string DeviceToken { get; set; }
}