using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;

public class EmailHandler : Handler
{
    private readonly NotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailHandler> _logger;

    public EmailHandler(NotificationService notificationService, IConfiguration configuration, ILogger<EmailHandler> logger)
    {
        Type = NotificationType.Email;
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
    }

    public override async void HandleTask(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting email notification processing for notification {NotificationId}", notification.Id);

            // Update status to Processing
            notification.Status = NotificationStatus.Processing;
            await _notificationService.UpdateAsync(notification);

            var recipientEmail = notification.User.Email;
            if (string.IsNullOrEmpty(recipientEmail))
            {
                throw new InvalidOperationException("Recipient email is not specified");
            }

            await SendEmailAsync(notification, recipientEmail, cancellationToken);

            // Update status to Sent
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _notificationService.UpdateAsync(notification);

            _logger.LogInformation("Email notification {NotificationId} sent successfully to {Email}", notification.Id, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification {NotificationId}", notification.Id);

            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            await _notificationService.UpdateAsync(notification);
        }
    }

    private async Task SendEmailAsync(Notification notification, string recipientEmail, CancellationToken cancellationToken)
    {
        var smtpSettings = _configuration.GetSection("EmailSettings");
        var smtpUser = smtpSettings["SmtpUser"];
        var smtpPassword = smtpSettings["SmtpPassword"];

        if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
        {
            throw new InvalidOperationException("SMTP credentials are required for email notifications. Please configure SmtpUser and SmtpPassword in EmailSettings.");
        }

        using var client = new SmtpClient();

        try
        {
            var smtpHost = smtpSettings["SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");


            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);

            await client.AuthenticateAsync(smtpUser, smtpPassword, cancellationToken);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Notification Service", smtpUser));
            message.To.Add(new MailboxAddress(notification.User.Username, recipientEmail));
            message.Subject = notification.Subject ?? "Notification";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = notification.Body;

            if (notification.Medias != null && notification.Medias.Any())
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
        catch
        {
            client.Dispose();
            throw;
        }
    }

    private async Task SendMockEmailAsync(Notification notification, string recipientEmail, IConfigurationSection smtpSettings, CancellationToken cancellationToken)
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