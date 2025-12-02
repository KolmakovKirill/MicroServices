using Notification.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

string rabbitMqHost = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "localhost";
string rabbitMqUser = Environment.GetEnvironmentVariable("RabbitMQ__User") ?? "guest";
string rabbitMqPass = Environment.GetEnvironmentVariable("RabbitMQ__Pass") ?? "guest";

var factory = new ConnectionFactory() { HostName = rabbitMqHost, UserName = rabbitMqUser, Password = rabbitMqPass };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "sms_notifications",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

Console.WriteLine("[SmsWorker] Waiting for messages in 'sms_notifications'. Press CTRL+C to exit.");

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
            SendSms(notification);
            Console.WriteLine($"[SmsWorker][STUB] SMS sent to {notification.Recipient}: {notification.Subject}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SmsWorker][ERROR]: {ex.Message}");
    }
};

channel.BasicConsume(queue: "sms_notifications",
                     autoAck: true,
                     consumer: consumer);

Console.CancelKeyPress += (sender, e) => {
    Console.WriteLine("[SmsWorker] Stopping...");
};

while(true) await Task.Delay(1000);

void SendSms(NotificationRequest req)
{
    // Это только заглушка! Логируем, но не отправляем реально.
    System.Threading.Thread.Sleep(150); // эмуляция отправки
}
