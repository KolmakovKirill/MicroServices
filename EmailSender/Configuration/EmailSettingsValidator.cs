using Microsoft.Extensions.Options;

namespace EmailSender.Configuration;

public sealed class EmailSettingsValidator : IValidateOptions<EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, EmailSettings options)
    {
        if (options.UseMock)
            return ValidateOptionsResult.Success;

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
            return ValidateOptionsResult.Fail("EmailSettings.SmtpHost is required");

        if (options.SmtpPort is < 1 or > 65535)
            return ValidateOptionsResult.Fail("EmailSettings.SmtpPort must be between 1 and 65535");

        if (string.IsNullOrWhiteSpace(options.SmtpUser))
            return ValidateOptionsResult.Fail("EmailSettings.SmtpUser is required");

        if (string.IsNullOrWhiteSpace(options.SmtpPassword))
            return ValidateOptionsResult.Fail("EmailSettings.SmtpPassword is required");

        return ValidateOptionsResult.Success;
    }
}