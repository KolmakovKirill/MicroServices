using System.Reactive;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using Confluent.Kafka;

namespace CommonShared.Infrastructure.Messaging.Services;


public class KafkaConsumerService<HandlerClass> : BackgroundService 
    where HandlerClass : Handler
{
    private readonly ILogger<KafkaConsumerService<HandlerClass>> _logger;
    private readonly string _topic = "notifications";
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private IConsumer<string, long>? _consumer;

    public KafkaConsumerService(IConfiguration config,
                                ILogger<KafkaConsumerService<HandlerClass>> logger,
                                IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            // Initialize consumer in ExecuteAsync to ensure network is ready
            var bootstrapServers = _config["Kafka:BootstrapServers"];
            var groupId = _config["Kafka:GroupId"];

            _logger.LogInformation("Initializing Kafka consumer with BootstrapServers: {BootstrapServers}, GroupId: {GroupId}", bootstrapServers, groupId);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, long>(consumerConfig).Build();
            _consumer.Subscribe(_topic);

            _logger.LogInformation("Kafka consumer initialized for topic {Topic}", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_consumer == null) break;

                    var cr = _consumer.Consume(stoppingToken);
                    var notificationId = cr.Message.Value;
                    var scope = _serviceProvider.CreateScope();

                    var _notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    var handler = scope.ServiceProvider.GetRequiredService<HandlerClass>();

                    var notification = await _notificationService.FindAsync(notificationId);
                    if (notification == null)
                    {
                        _logger.LogWarning("Не обнаружено уведомления с таким ID!");
                        continue;
                    }

                    if (handler.Type == notification.Type)
                    {
                        handler.HandleTask(notification, stoppingToken);
                    }
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
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}
