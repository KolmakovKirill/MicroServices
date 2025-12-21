namespace EmailSender.Configuration;

public sealed class EmailSettings
{
    public bool UseMock { get; set; } = false;

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;

    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;

    public string SenderName { get; set; } = "Notification Service";
}