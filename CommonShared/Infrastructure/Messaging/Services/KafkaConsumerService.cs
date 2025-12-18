using System.Reactive;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using CommonShared.Infrastructure.Messaging.Models;
using CommonShared.Infrastructure.Messaging.Serializers;
using Confluent.Kafka;

namespace CommonShared.Infrastructure.Messaging.Services;

public class KafkaConsumerService<HandlerClass> : BackgroundService 
    where HandlerClass : Handler
{
    private readonly ILogger<KafkaConsumerService<HandlerClass>> _logger;
    private readonly string _topic = "notifications";
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private IConsumer<string, NotificationMessage>? _consumer;

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
            var bootstrapServers = _config["Kafka:BootstrapServers"];
            var groupId = _config["Kafka:GroupId"];

            _logger.LogInformation("Initializing Kafka consumer with BootstrapServers: {BootstrapServers}, GroupId: {GroupId}", 
                bootstrapServers, groupId);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, NotificationMessage>(consumerConfig)
                .SetValueDeserializer(new JsonDeserializer<NotificationMessage>())
                .Build();
            
            _consumer.Subscribe(_topic);

            _logger.LogInformation("Kafka consumer initialized for topic {Topic}", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_consumer == null) break;

                    var cr = _consumer.Consume(stoppingToken);
                    var notificationMessage = cr.Message.Value;
                    var notificationId = notificationMessage.NotificationId;
                    var notificationType = notificationMessage.NotificationType;

                    var scope = _serviceProvider.CreateScope();

                    var handler = scope.ServiceProvider.GetRequiredService<HandlerClass>();
                    if (handler.Type.ToString() == notificationType)
                    {
                        var _notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                        var notification = await _notificationService.FindAsync(notificationId);
                        if (notification == null)
                        {
                            _logger.LogWarning("Не обнаружено уведомления с таким ID: {NotificationId}", notificationId);
                            return;
                        }
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