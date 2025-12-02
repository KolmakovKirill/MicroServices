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

channel.QueueDeclare(queue: "push_notifications",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

Console.WriteLine("[PushWorker] Waiting for messages in 'push_notifications'. Press CTRL+C to exit.");

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
            SendPush(notification);
            Console.WriteLine($"[PushWorker] Push sent to {notification.Recipient}: {notification.Subject}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[PushWorker][ERROR]: {ex.Message}");
    }
};

channel.BasicConsume(queue: "push_notifications",
                     autoAck: true,
                     consumer: consumer);

Console.CancelKeyPress += (sender, e) => {
    Console.WriteLine("[PushWorker] Stopping...");
};

while(true) await Task.Delay(1000);

void SendPush(NotificationRequest req)
{
    // Имитация отправки push, можно заменить на запрос к реальному push API
    // Здесь просто логируется факт доставки (боевой worker)
    System.Threading.Thread.Sleep(500); // эмуляция задержки
}
