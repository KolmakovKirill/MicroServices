namespace SmsSender.Configuration;

using Microsoft.Extensions.Options;

public sealed class SmsSettingsValidator : IValidateOptions<SmsSettings>
{
    public ValidateOptionsResult Validate(string? name, SmsSettings options)
    {
        if (options.Provider == SmsProvider.Mock)
            return ValidateOptionsResult.Success;

        if (options.Provider == SmsProvider.Twilio)
        {
            if (string.IsNullOrWhiteSpace(options.Twilio.AccountSid))
                return ValidateOptionsResult.Fail("SmsSettings.Twilio.AccountSid is required");

            if (string.IsNullOrWhiteSpace(options.Twilio.AuthToken))
                return ValidateOptionsResult.Fail("SmsSettings.Twilio.AuthToken is required");

            if (string.IsNullOrWhiteSpace(options.Twilio.FromNumber))
                return ValidateOptionsResult.Fail("SmsSettings.Twilio.FromNumber is required");

            return ValidateOptionsResult.Success;
        }

        if (options.Provider == SmsProvider.AwsSns)
        {
            if (string.IsNullOrWhiteSpace(options.AwsSns.Region))
                return ValidateOptionsResult.Fail("SmsSettings.AwsSns.Region is required");

            if (string.IsNullOrWhiteSpace(options.AwsSns.AccessKeyId))
                return ValidateOptionsResult.Fail("SmsSettings.AwsSns.AccessKeyId is required");

            if (string.IsNullOrWhiteSpace(options.AwsSns.SecretAccessKey))
                return ValidateOptionsResult.Fail("SmsSettings.AwsSns.SecretAccessKey is required");

            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail("Unknown SmsProvider");
    }
}
