using Microsoft.Extensions.Options;

namespace PushSender.Configuration;

public sealed class FirebaseSettingsValidator : IValidateOptions<FirebaseSettings>
{
    public ValidateOptionsResult Validate(string? name, FirebaseSettings options)
    {
        if (options.UseMock)
            return ValidateOptionsResult.Success;

        if (string.IsNullOrWhiteSpace(options.CredentialsPath))
            return ValidateOptionsResult.Fail("FirebaseSettings.CredentialsPath is required when UseMock = false");

        // if (!File.Exists(options.CredentialsPath))
        //     return ValidateOptionsResult.Fail($"FirebaseSettings.CredentialsPath file not found: {options.CredentialsPath}");

        if (string.IsNullOrWhiteSpace(options.AppName))
            return ValidateOptionsResult.Fail("FirebaseSettings.AppName is required");

        return ValidateOptionsResult.Success;
    }
}