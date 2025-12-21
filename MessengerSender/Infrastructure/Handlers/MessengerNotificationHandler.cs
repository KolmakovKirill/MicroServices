using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using MessengerSender.Configuration;
using Microsoft.Extensions.Options;

namespace MessengerSender.Infrastructure.Handlers;

public class MessengerNotificationHandler : NotificationHandler
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<MessengerNotificationHandler> _logger;
    private readonly HttpClient _httpClient;
    private readonly MessengerSettings _messengerSettings;

    public MessengerNotificationHandler(
        NotificationService notificationService,
        ILogger<MessengerNotificationHandler> logger,
        HttpClient httpClient,
        IOptions<MessengerSettings> messengerSettings)
    {
        Type = NotificationType.Messenger;
        _notificationService = notificationService;
        _logger = logger;
        _httpClient = httpClient;
        _messengerSettings = messengerSettings.Value;
    }

    public override async Task HandleTask(Notification notification, CancellationToken cancellationToken)
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
        switch (_messengerSettings.Provider)
        {
            case MessengerProvider.Telegram:
                await SendViaTelegramAsync(notification, messengerId, cancellationToken);
                break;
            case MessengerProvider.WhatsApp:
                await SendViaWhatsAppAsync(notification, messengerId, cancellationToken);
                break;
            default:
                await SendMockMessengerNotificationAsync(notification, messengerId, cancellationToken);
                break;
        }
    }

    private async Task SendViaTelegramAsync(Notification notification, string chatId, CancellationToken cancellationToken)
    {
        var botToken = _messengerSettings.Telegram.BotToken;

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

    private async Task SendViaWhatsAppAsync(Notification notification, string recipientId, CancellationToken cancellationToken)
    {
        var wa = _messengerSettings.WhatsApp;

        var url = $"https://graph.facebook.com/v19.0/{wa.PhoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = recipientId,
            type = "template",
            template = new
            {
                name = wa.TemplateName,
                language = new { code = wa.LanguageCode },
                components = new[]
                {
                    new
                    {
                        type = "body",
                        parameters = new object[]
                        {
                            new { type = "text", text = notification.Subject ?? "Notification" },
                            new { type = "text", text = notification.Body ?? "" }
                        }
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", wa.AccessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"WhatsApp API error: {(int)response.StatusCode} {responseBody}");

        _logger.LogInformation("WhatsApp message sent to {RecipientId}. Response: {Response}", recipientId, responseBody);
    }

    private async Task SendMockMessengerNotificationAsync(Notification notification, string messengerId, CancellationToken cancellationToken)
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