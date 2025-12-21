using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using Microsoft.Extensions.Options;
using SmsSender.Configuration;

namespace SmsSender.Infrastructure.Handlers;

public class SmsNotificationHandler : NotificationHandler
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<SmsNotificationHandler> _logger;
    private readonly HttpClient _httpClient;
    private readonly SmsSettings _smsSettings;

    public SmsNotificationHandler(NotificationService notificationService, ILogger<SmsNotificationHandler> logger, HttpClient httpClient, IOptions<SmsSettings> smsSettings)
    {
        Type = NotificationType.SMS;
        _notificationService = notificationService;
        _logger = logger;
        _httpClient = httpClient;
        _smsSettings = smsSettings.Value;
    }

    public override async Task HandleTask(Notification notification, CancellationToken cancellationToken)
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
        switch (_smsSettings.Provider)
        {
            case SmsProvider.Twilio:
                await SendViaTwilioAsync(notification, recipientPhone, cancellationToken);
                break;
            case SmsProvider.AwsSns:
                await SendViaAwsSnsAsync(notification, recipientPhone, cancellationToken);
                break;
            default:
                await SendMockSmsAsync(notification, recipientPhone, cancellationToken);
                break;
        }
    }

    private async Task SendViaTwilioAsync(Notification notification, string recipientPhone, CancellationToken cancellationToken)
    {
        var accountSid = _smsSettings.Twilio.AccountSid;
        var authToken = _smsSettings.Twilio.AuthToken;
        var fromNumber = _smsSettings.Twilio.FromNumber;

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

    private async Task SendViaAwsSnsAsync(
        Notification notification,
        string recipientPhone,
        CancellationToken cancellationToken)
    {
        var aws = _smsSettings.AwsSns;

        var credentials = new BasicAWSCredentials(
            aws.AccessKeyId,
            aws.SecretAccessKey);

        using var client = new AmazonSimpleNotificationServiceClient(
            credentials,
            RegionEndpoint.GetBySystemName(aws.Region));

        var request = new PublishRequest
        {
            PhoneNumber = recipientPhone,
            Message = notification.Body,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["AWS.SNS.SMS.SMSType"] = new()
                {
                    DataType = "String",
                    StringValue = "Transactional"
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(aws.SenderId))
        {
            request.MessageAttributes["AWS.SNS.SMS.SenderID"] =
                new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = aws.SenderId
                };
        }

        var response = await client.PublishAsync(request, cancellationToken);

        _logger.LogInformation(
            "AWS SNS SMS sent. MessageId={MessageId}",
            response.MessageId);
    }

    private async Task SendMockSmsAsync(Notification notification, string recipientPhone, CancellationToken cancellationToken)
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