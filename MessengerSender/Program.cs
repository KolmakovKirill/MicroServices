using CommonShared.Configuration;
using Minio;
using CommonShared.Infrastructure.DataStorage;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using CommonShared.Observability;
using MessengerSender.Configuration;
using MessengerSender.Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMinio(configureClient => configureClient
                                                .WithEndpoint("minio:9000")
                                                .WithCredentials("miniouser", "admin123")
                                                .WithSSL(false)
                                                .Build());

builder.Services.AddDbContext<ServerDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("CommonShared")
    );
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<MessengerNotificationHandler>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddLogging();
builder.Services.AddHostedService<KafkaConsumerService<MessengerNotificationHandler>>();
builder.Services.AddKafkaConsumerSettings(builder.Configuration, "Kafka");

builder.Services.AddSingleton<IValidateOptions<MessengerSettings>, MessengerSettingsValidator>();

builder.Services
    .AddOptions<MessengerSettings>()
    .Bind(builder.Configuration.GetSection("MessengerSettings"))
    .ValidateOnStart();

var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "MessengerSender";
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? throw new NullReferenceException("OTEL_EXPORTER_OTLP_ENDPOINT");

builder.Services
    .AddCommonObservability(serviceName, otlpEndpoint);

var app = builder.Build();
app.Run();
