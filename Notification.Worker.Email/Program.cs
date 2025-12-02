using Notification.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Net.Mail;

string rabbitMqHost = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "localhost";
string rabbitMqUser = Environment.GetEnvironmentVariable("RabbitMQ__User") ?? "guest";
string rabbitMqPass = Environment.GetEnvironmentVariable("RabbitMQ__Pass") ?? "guest";

string smtpHost = Environment.GetEnvironmentVariable("SMTP__Host") ?? "localhost";
int smtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP__Port"), out var port) ? port : 25;
string smtpUser = Environment.GetEnvironmentVariable("SMTP__User") ?? string.Empty;
string smtpPass = Environment.GetEnvironmentVariable("SMTP__Pass") ?? string.Empty;

var factory = new ConnectionFactory() { HostName = rabbitMqHost, UserName = rabbitMqUser, Password = rabbitMqPass };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "email_notifications",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

Console.WriteLine("[EmailWorker] Waiting for messages in 'email_notifications'. Press CTRL+C to exit.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    try
    {
        var notification = JsonSerializer.Deserialize<NotificationRequest>(message);
        if (notification != null)
        {
            SendEmail(notification);
            Console.WriteLine($"[EmailWorker] Sent to {notification.Recipient}: {notification.Subject}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[EmailWorker][ERROR]: {ex.Message}");
    }
};

channel.BasicConsume(queue: "email_notifications",
                    autoAck: true,
                    consumer: consumer);

Console.CancelKeyPress += (sender, e) => {
    Console.WriteLine("[EmailWorker] Stopping...");
};

// Hang while running
task: while(true) await Task.Delay(1000);

void SendEmail(NotificationRequest req)
{
    using var client = new SmtpClient(smtpHost, smtpPort)
    {
        EnableSsl = false,
        Credentials = string.IsNullOrWhiteSpace(smtpUser)
            ? null
            : new System.Net.NetworkCredential(smtpUser, smtpPass)
    };
    var mail = new MailMessage(from: smtpUser, to: req.Recipient, subject: req.Subject, body: req.Message);
    client.Send(mail);
}
