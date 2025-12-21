namespace SmsSender.Configuration;

public sealed class SmsSettings
{
    public SmsProvider Provider { get; set; } = SmsProvider.Mock;

    public TwilioSmsSettings Twilio { get; set; } = new();
    public AwsSnsSmsSettings AwsSns { get; set; } = new();
}

public sealed class TwilioSmsSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public sealed class AwsSnsSmsSettings
{
    public string Region { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
}

public enum SmsProvider
{
    Twilio = 0,
    AwsSns = 1,
    Mock = 2
}