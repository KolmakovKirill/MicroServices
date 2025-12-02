using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace microservices_project.Infrastructure.Messaging.Services;


public static class KafkaTopicInitializer
{
    public static async Task EnsureTopicsAsync(IConfiguration config)
    {
        var bootstrap = config["Kafka:BootstrapServers"];
        var topics = new[]
        {
            new TopicSpecification
            {
                Name = "notifications",
                NumPartitions = 3,
                ReplicationFactor = 1
            }
        };

        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = bootstrap
        }).Build();

        try
        {
            await admin.CreateTopicsAsync(topics);
        }
        catch (CreateTopicsException ex)
        {
            foreach (var r in ex.Results)
            {
                if (r.Error.Code == ErrorCode.TopicAlreadyExists)
                    continue; // норм – топик уже есть
                throw;
            }
        }
    }
}
