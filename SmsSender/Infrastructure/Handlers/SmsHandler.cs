using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

public class SmsHandler : Handler
{
    private readonly NotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsHandler> _logger;
    private readonly HttpClient _httpClient;

    public SmsHandler(NotificationService notificationService, IConfiguration configuration, ILogger<SmsHandler> logger, HttpClient httpClient)
    {
        Type = NotificationType.SMS;
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public override async void HandleTask(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting SMS notification processing for notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Processing;
            await _notificationService.UpdateAsync(notification);

            var recipientPhone = notification.User.PhoneNumber;
            if (string.IsNullOrEmpty(recipientPhone))
            {
                throw new InvalidOperationException("Recipient phone number is not specified");
            }

            await SendSmsAsync(notification, recipientPhone, cancellationToken);

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _notificationService.UpdateAsync(notification);

            _logger.LogInformation("SMS notification {NotificationId} sent successfully to {PhoneNumber}", notification.Id, recipientPhone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            await _notificationService.UpdateAsync(notification);
        }
    }

    private async Task SendSmsAsync(Notification notification, string recipientPhone, CancellationToken cancellationToken)
    {
        var smsSettings = _configuration.GetSection("SmsSettings");
        var provider = smsSettings["Provider"] ?? "twilio";

        switch (provider.ToLower())
        {
            case "twilio":
                await SendViaTwilioAsync(notification, recipientPhone, smsSettings, cancellationToken);
                break;
            case "aws-sns":
                await SendViaAwsSnsAsync(notification, recipientPhone, smsSettings, cancellationToken);
                break;
            default:
                await SendMockSmsAsync(notification, recipientPhone, smsSettings, cancellationToken);
                break;
        }
    }

    private async Task SendViaTwilioAsync(Notification notification, string recipientPhone, IConfigurationSection smsSettings, CancellationToken cancellationToken)
    {
        var accountSid = smsSettings["AccountSid"];
        var authToken = smsSettings["AuthToken"];
        var fromNumber = smsSettings["FromNumber"];

        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
        {
            throw new InvalidOperationException("Twilio credentials not configured");
        }

        var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("To", recipientPhone),
            new KeyValuePair<string, string>("From", fromNumber),
            new KeyValuePair<string, string>("Body", notification.Body)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{accountSid}:{authToken}"));
        request.Headers.Add("Authorization", $"Basic {credentials}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Twilio SMS sent. Response: {Response}", responseContent);
    }

    private async Task SendViaAwsSnsAsync(Notification notification, string recipientPhone, IConfigurationSection smsSettings, CancellationToken cancellationToken)
    {

        await Task.Delay(500, cancellationToken);
        _logger.LogInformation("AWS SNS SMS would be sent to {PhoneNumber}", recipientPhone);
    }

    private async Task SendMockSmsAsync(Notification notification, string recipientPhone, IConfigurationSection smsSettings, CancellationToken cancellationToken)
    {
        await Task.Delay(200, cancellationToken);

        if (new Random().Next(100) < 10)
        {
            throw new Exception("Simulated SMS sending failure");
        }

        _logger.LogInformation("Mock SMS sent successfully to {PhoneNumber} with message: {Message}",
            recipientPhone, notification.Body);
    }
}
