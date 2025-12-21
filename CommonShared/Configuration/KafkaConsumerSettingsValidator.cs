using Microsoft.Extensions.Options;

namespace CommonShared.Configuration;

public sealed class KafkaConsumerSettingsValidator : IValidateOptions<KafkaConsumerSettings>
{
    public ValidateOptionsResult Validate(string? name, KafkaConsumerSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.BootstrapServers))
            return ValidateOptionsResult.Fail("Kafka:BootstrapServers is required");

        if (string.IsNullOrWhiteSpace(options.GroupId))
            return ValidateOptionsResult.Fail("Kafka:GroupId is required");

        if (string.IsNullOrWhiteSpace(options.Topic))
            return ValidateOptionsResult.Fail("Kafka:Topic is required");

        if (options.MaxParallel <= 0)
            return ValidateOptionsResult.Fail("Kafka:MaxParallel must be > 0");

        return ValidateOptionsResult.Success;
    }
}