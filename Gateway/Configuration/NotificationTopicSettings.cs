namespace microservices_project.Configuration;

public sealed class NotificationTopicsSettings
{
    public string Sms { get; set; } = "notifications.sms";
    public string Email { get; set; } = "notifications.email";
    public string Push { get; set; } = "notifications.push";
    public string Messenger { get; set; } = "notifications.messenger";
}
