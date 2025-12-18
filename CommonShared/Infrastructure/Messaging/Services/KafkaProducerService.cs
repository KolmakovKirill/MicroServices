using Confluent.Kafka;
using CommonShared.Infrastructure.Messaging.Models;
using CommonShared.Infrastructure.Messaging.Serializers;
using CommonShared.Core.Domain;

namespace CommonShared.Infrastructure.Messaging.Services;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<string, NotificationMessage> _producer;

    public KafkaProducerService(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, NotificationMessage>(producerConfig)
            .SetValueSerializer(new JsonSerializer<NotificationMessage>())
            .Build();
    }

    public async Task SendAsync(string topic, long notificationId, NotificationType notificationType, CancellationToken ct = default)
    {
        var message = new NotificationMessage
        {
            NotificationId = notificationId,
            NotificationType = notificationType.ToString()
        };

        await _producer.ProduceAsync(
            topic,
            new Message<string, NotificationMessage> 
            { 
                Key = notificationId.ToString(), 
                Value = message 
            },
            ct);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}