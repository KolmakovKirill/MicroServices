namespace PushSender.Configuration;

public sealed class FirebaseSettings
{
    public bool UseMock { get; set; } = true;
    public string CredentialsPath { get; set; } = string.Empty;
    public string AppName { get; set; } = "PushSender";
}