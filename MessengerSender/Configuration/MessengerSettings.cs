namespace MessengerSender.Configuration;

public sealed class MessengerSettings
{
    public MessengerProvider Provider { get; set; }
    
    public TelegramSettings Telegram { get; set; } = new();
    public WhatsAppSettings WhatsApp { get; set; } = new();
}

public sealed class TelegramSettings
{
    public string BotToken { get; set; } = string.Empty;
}

public sealed class WhatsAppSettings
{
    public string AccessToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en_US";
}

public enum MessengerProvider
{
    Telegram = 0,
    WhatsApp = 1,
    Mock = 2
}