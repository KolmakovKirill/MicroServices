using System.Text.Json;
using Confluent.Kafka;

namespace microservices_project.Infrastructure.Messaging.Services;


public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConsumer<Null, string> _consumer;
    private readonly string _topic = "notifications";

    public KafkaConsumerService(IConfiguration config,
                                ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        _consumer.Subscribe(_topic);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            { // Тут пока что просто для наглядности показывается сообщение само, потом будет сериализация/десериализация и тд...
                try
                {
                    var cr = _consumer.Consume(stoppingToken);

                    // JSON вида: {"action":"...","body":...,"created_at":"...","notification_id":"..."} 
                    var json = cr.Message.Value;
                    // var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    // var action = dict?["action"].GetString();
                    // var notificationId = dict?["notification_id"].GetString();

                    // _logger.LogInformation(
                    //     "Got message. Action={Action}, NotificationId={Id}, Offset={Offset}",
                    //     action, notificationId, cr.TopicPartitionOffset);
                    Console.WriteLine(json);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while consuming Kafka message");
                }
            }
        }, stoppingToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
