using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Notification.Shared;
using RabbitMQ.Client;

namespace Notification.Api.Services
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Dictionary<NotificationChannel, string> _queues = new()
        {
            { NotificationChannel.Email, "email_notifications" },
            { NotificationChannel.Push, "push_notifications" },
            { NotificationChannel.Sms, "sms_notifications" },
        };

        public RabbitMqPublisher(IConfiguration config)
        {
            var factory = new ConnectionFactory()
            {
                HostName = config["RabbitMQ:Host"] ?? "localhost",
                UserName = config["RabbitMQ:User"] ?? "guest",
                Password = config["RabbitMQ:Pass"] ?? "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            foreach (var q in _queues.Values)
                _channel.QueueDeclare(q, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public Task PublishAsync(NotificationRequest req)
        {
            var queue = _queues[req.Channel];
            var payload = JsonSerializer.Serialize(req);
            var body = Encoding.UTF8.GetBytes(payload);
            _channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
