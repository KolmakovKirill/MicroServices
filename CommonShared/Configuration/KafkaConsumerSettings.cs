using Microsoft.Extensions.Options;

namespace CommonShared.Configuration;

using Confluent.Kafka;

public sealed class KafkaConsumerSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;

    public int MaxParallel { get; set; } = 8;

    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
    public bool EnableAutoCommit { get; set; } = true;
}

public static class KafkaConsumerSettingsExtensions
{
    public static IServiceCollection AddKafkaConsumerSettings(this IServiceCollection services, IConfiguration configuration, string kafkaSectionName)
    {
        services.AddSingleton<IValidateOptions<KafkaConsumerSettings>, KafkaConsumerSettingsValidator>();

        services
            .AddOptions<KafkaConsumerSettings>()
            .Bind(configuration.GetSection(kafkaSectionName))
            .ValidateOnStart();

        return services;
    }
}
