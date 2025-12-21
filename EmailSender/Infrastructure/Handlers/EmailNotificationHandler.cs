using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using EmailSender.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailSender.Infrastructure.Handlers;

public class EmailNotificationHandler : NotificationHandler
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<EmailNotificationHandler> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailNotificationHandler(NotificationService notificationService, IOptions<EmailSettings> emailSettings,
        ILogger<EmailNotificationHandler> logger)
    {
        Type = NotificationType.Email;
        _notificationService = notificationService;
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public override async Task HandleTask(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting email notification processing for notification {NotificationId}",
                notification.Id);

            // Update status to Processing
            notification.Status = NotificationStatus.Processing;
            await _notificationService.UpdateAsync(notification);

            var recipientEmail = notification.User.Email;
            if (string.IsNullOrEmpty(recipientEmail))
            {
                throw new InvalidOperationException("Recipient email is not specified");
            }

            switch (_emailSettings.UseMock)
            {
                case true:
                    await SendMockEmailAsync(notification, recipientEmail, cancellationToken);
                    break;
                case false:
                    await SendEmailAsync(notification, recipientEmail, cancellationToken);
                    break;
            }

            // Update status to Sent
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _notificationService.UpdateAsync(notification);

            _logger.LogInformation("Email notification {NotificationId} sent successfully to {Email}", notification.Id,
                recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            await _notificationService.UpdateAsync(notification);
        }
    }

    private async Task SendEmailAsync(Notification notification, string recipientEmail,
        CancellationToken cancellationToken)
    {
        var smtpUser = _emailSettings.SmtpUser;
        var smtpPassword = _emailSettings.SmtpPassword;

        using var client = new SmtpClient();

        var smtpHost = _emailSettings.SmtpHost;
        var smtpPort = _emailSettings.SmtpPort;

        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);

        await client.AuthenticateAsync(smtpUser, smtpPassword, cancellationToken);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, smtpUser));
        message.To.Add(new MailboxAddress(notification.User.Username, recipientEmail));
        message.Subject = notification.Subject ?? "Notification";

        var bodyBuilder = new BodyBuilder
        {
            TextBody = notification.Body
        };

        if (notification.Medias.Any())
        {
            foreach (var media in notification.Medias)
            {
                _logger.LogInformation("Attachment {MediaId} would be included in email", media.Id);
            }
        }

        message.Body = bodyBuilder.ToMessageBody();

        // Send email
        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }

    private async Task SendMockEmailAsync(Notification notification, string recipientEmail,
        CancellationToken cancellationToken)
    {
        await Task.Delay(300, cancellationToken);

        if (new Random().Next(100) < 5)
        {
            throw new Exception("Simulated email sending failure");
        }

        _logger.LogInformation("Mock email sent successfully to {Email} with subject: '{Subject}', body: '{Body}'",
            recipientEmail, notification.Subject, notification.Body);
    }
}