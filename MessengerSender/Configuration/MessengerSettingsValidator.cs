using Microsoft.Extensions.Options;

namespace MessengerSender.Configuration;

public sealed class MessengerSettingsValidator
    : IValidateOptions<MessengerSettings>
{
    public ValidateOptionsResult Validate(
        string? name,
        MessengerSettings options)
    {
        return options.Provider switch
        {
            MessengerProvider.Telegram =>
                string.IsNullOrWhiteSpace(options.Telegram.BotToken)
                    ? ValidateOptionsResult.Fail("Telegram.BotToken is required")
                    : ValidateOptionsResult.Success,

            MessengerProvider.WhatsApp =>
                string.IsNullOrWhiteSpace(options.WhatsApp.AccessToken) ||
                string.IsNullOrWhiteSpace(options.WhatsApp.PhoneNumberId) ||
                string.IsNullOrWhiteSpace(options.WhatsApp.TemplateName)
                    ? ValidateOptionsResult.Fail("WhatsApp settings are incomplete")
                    : ValidateOptionsResult.Success,

            MessengerProvider.Mock =>
                ValidateOptionsResult.Success,

            _ => ValidateOptionsResult.Fail("Unknown MessengerProvider")
        };
    }
}