using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

public class MessengerHandler : Handler
{
    private readonly NotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessengerHandler> _logger;
    private readonly HttpClient _httpClient;

    public MessengerHandler(NotificationService notificationService, IConfiguration configuration, ILogger<MessengerHandler> logger, HttpClient httpClient)
    {
        Type = NotificationType.Messenger;
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public override async void HandleTask(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting messenger notification processing for notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Processing;
            await _notificationService.UpdateAsync(notification);

            var messengerId = notification.User.MessengerId;
            if (string.IsNullOrEmpty(messengerId))
            {
                throw new InvalidOperationException("Messenger ID is not specified");
            }

            await SendMessengerNotificationAsync(notification, messengerId, cancellationToken);

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _notificationService.UpdateAsync(notification);

            _logger.LogInformation("Messenger notification {NotificationId} sent successfully to {MessengerId}", notification.Id, messengerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send messenger notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            await _notificationService.UpdateAsync(notification);
        }
    }

    private async Task SendMessengerNotificationAsync(Notification notification, string messengerId, CancellationToken cancellationToken)
    {
        var messengerSettings = _configuration.GetSection("MessengerSettings");
        var provider = messengerSettings["Provider"] ?? "telegram"; 

        switch (provider.ToLower())
        {
            case "telegram":
                await SendViaTelegramAsync(notification, messengerId, messengerSettings, cancellationToken);
                break;
            case "whatsapp":
                await SendViaWhatsAppAsync(notification, messengerId, messengerSettings, cancellationToken);
                break;
            default:
                await SendMockMessengerNotificationAsync(notification, messengerId, messengerSettings, cancellationToken);
                break;
        }
    }

    private async Task SendViaTelegramAsync(Notification notification, string chatId, IConfigurationSection messengerSettings, CancellationToken cancellationToken)
    {
        var botToken = messengerSettings["BotToken"];

        if (string.IsNullOrEmpty(botToken))
        {
            throw new InvalidOperationException("Telegram bot token not configured");
        }

        var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

        var messageText = $"*{notification.Subject ?? "Notification"}*\n\n{notification.Body}";

        messageText = messageText.Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`");

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("chat_id", chatId),
            new KeyValuePair<string, string>("text", messageText),
            new KeyValuePair<string, string>("parse_mode", "Markdown")
        });

        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var telegramResponse = JsonSerializer.Deserialize<TelegramResponse>(responseContent);

        if (telegramResponse?.ok != true)
        {
            throw new Exception($"Telegram API error: {telegramResponse?.description ?? "Unknown error"}");
        }

        _logger.LogInformation("Telegram message sent successfully. Message ID: {MessageId}", telegramResponse.result?.message_id);
    }

    private async Task SendViaWhatsAppAsync(Notification notification, string recipientId, IConfigurationSection messengerSettings, CancellationToken cancellationToken)
    {
        await Task.Delay(400, cancellationToken);
        _logger.LogInformation("WhatsApp message would be sent to {RecipientId}", recipientId);
    }

    private async Task SendMockMessengerNotificationAsync(Notification notification, string messengerId, IConfigurationSection messengerSettings, CancellationToken cancellationToken)
    {
        await Task.Delay(250, cancellationToken);

        if (new Random().Next(100) < 8) 
        {
            throw new Exception("Simulated messenger notification sending failure");
        }

        _logger.LogInformation("Mock messenger notification sent successfully to {MessengerId} with subject: {Subject}, message: {Message}",
            messengerId, notification.Subject, notification.Body);
    }

    private class TelegramResponse
    {
        public bool ok { get; set; }
        public string? description { get; set; }
        public TelegramResult? result { get; set; }
    }

    private class TelegramResult
    {
        public int message_id { get; set; }
    }
}
