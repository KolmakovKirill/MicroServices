using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using PushSender.Configuration;

namespace PushSender.Services;

public sealed class FirebaseAppProvider : IFirebaseAppProvider
{
    private readonly FirebaseSettings _settings;
    private readonly ILogger<FirebaseAppProvider> _logger;
    private FirebaseApp? _app;
    private bool _initialized;

    public FirebaseAppProvider(IOptions<FirebaseSettings> options, ILogger<FirebaseAppProvider> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public FirebaseApp? TryGetApp()
    {
        if (_initialized)
            return _app;

        _initialized = true;

        if (_settings.UseMock)
            return null;

        try
        {
            _app = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(_settings.CredentialsPath)
            }, _settings.AppName);

            return _app;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FirebaseApp");
            return null;
        }
    }
}