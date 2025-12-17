using System.Reactive;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using Confluent.Kafka;

namespace CommonShared.Infrastructure.Messaging.Services;


public class KafkaConsumerService<HandlerClass> : BackgroundService 
    where HandlerClass : Handler
{
    private readonly ILogger<KafkaConsumerService<HandlerClass>> _logger;
    private readonly IConsumer<string, long> _consumer;
    private readonly string _topic = "notifications";
    // private readonly NotificationService _notificationService;
    private readonly IServiceProvider _serviceProvider;
    // private readonly HandlerClass _handler;

    public KafkaConsumerService(IConfiguration config,
                                ILogger<KafkaConsumerService<HandlerClass>> logger,
                                IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = config["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<string, long>(consumerConfig).Build();
        _consumer.Subscribe(_topic);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(stoppingToken); 
                    var notificationId = cr.Message.Value;
                    var scope = _serviceProvider.CreateScope();

                    var _notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    var handler = scope.ServiceProvider.GetRequiredService<HandlerClass>(); 

                    var notification = await _notificationService.FindAsync(notificationId);
                    if (notification == null)
                    {
                        _logger.Log(LogLevel.Warning, "Не обнаружено уведомления с таким ID!");
                    }
                    
                    if (handler.Type == notification.Type) // Тут можно будет далее поменять логику, так как уже угодно
                    {
                        handler.HandleTask(notification, stoppingToken); // Тут уже можно подправить на то, как вы хотите сделать
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
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
