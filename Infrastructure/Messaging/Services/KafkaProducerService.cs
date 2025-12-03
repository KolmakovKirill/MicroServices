using Confluent.Kafka;

namespace microservices_project.Infrastructure.Messaging.Services;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build(); 
    }

    public async Task SendAsync(string topic, string message, CancellationToken ct = default)
    {
        var result = await _producer.ProduceAsync(
            topic,
            new Message<Null, string> { Value = message },
            ct);

        Console.WriteLine($"Sent to {result.TopicPartitionOffset}");
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}