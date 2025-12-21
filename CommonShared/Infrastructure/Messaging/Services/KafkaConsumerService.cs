using CommonShared.Configuration;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Handlers;
using CommonShared.Infrastructure.Messaging.Models;
using CommonShared.Infrastructure.Messaging.Serializers;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace CommonShared.Infrastructure.Messaging.Services;

public class KafkaConsumerService<THandlerClass> : BackgroundService
    where THandlerClass : NotificationHandler
{
    private readonly ILogger<KafkaConsumerService<THandlerClass>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConsumer<string, NotificationMessage>? _consumer;
    private readonly KafkaConsumerSettings _settings;

    public KafkaConsumerService(
        ILogger<KafkaConsumerService<THandlerClass>> logger,
        IServiceProvider serviceProvider,
        IOptions<KafkaConsumerSettings> consumerSettings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = consumerSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Initializing Kafka consumer with BootstrapServers: {BootstrapServers}, GroupId: {GroupId}, Topic: {Topic}, MaxParallel: {MaxParallel}",
            _settings.BootstrapServers, _settings.GroupId, _settings.Topic, _settings.MaxParallel);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = _settings.AutoOffsetReset,
            EnableAutoCommit = _settings.EnableAutoCommit
        };

        _consumer = new ConsumerBuilder<string, NotificationMessage>(consumerConfig)
            .SetValueDeserializer(new JsonDeserializer<NotificationMessage>())
            .Build();

        _consumer.Subscribe(_settings.Topic);

        using var semaphore = new SemaphoreSlim(_settings.MaxParallel, _settings.MaxParallel);
        var inFlight = new List<Task>(capacity: _settings.MaxParallel * 2);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_consumer == null) break;

                var cr = _consumer.Consume(stoppingToken);
                var msg = cr.Message.Value;

                await semaphore.WaitAsync(stoppingToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();

                        var handler = scope.ServiceProvider.GetRequiredService<THandlerClass>();

                        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                        var notification = await notificationService.FindAsync(msg.NotificationId);

                        if (notification == null)
                        {
                            _logger.LogWarning("Не обнаружено уведомления с таким ID: {NotificationId}", msg.NotificationId);
                            return;
                        }

                        await handler.HandleTask(notification, stoppingToken);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while processing Kafka message");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, stoppingToken);

                inFlight.Add(task);

                for (var i = inFlight.Count - 1; i >= 0; i--)
                {
                    if (inFlight[i].IsCompleted)
                        inFlight.RemoveAt(i);
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

        try
        {
            await Task.WhenAll(inFlight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while awaiting in-flight tasks");
        }
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}
