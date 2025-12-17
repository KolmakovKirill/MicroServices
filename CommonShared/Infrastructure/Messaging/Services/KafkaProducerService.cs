using Confluent.Kafka;

namespace CommonShared.Infrastructure.Messaging.Services;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<string, long> _producer;

    public KafkaProducerService(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, long>(producerConfig).Build(); 
    }

    public async Task SendAsync(string topic, long notificationId, CancellationToken ct = default)
    {
        await _producer.ProduceAsync(
            topic,
            new Message<string, long> { Key = "notificationId", Value = notificationId },
            ct);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}