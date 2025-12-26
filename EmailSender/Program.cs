using CommonShared.Configuration;
using Minio;
using CommonShared.Infrastructure.DataStorage;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using CommonShared.Observability;
using EmailSender.Configuration;
using EmailSender.Infrastructure.Handlers;
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
builder.Services.AddScoped<EmailNotificationHandler>();
builder.Services.AddLogging();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHostedService<KafkaConsumerService<EmailNotificationHandler>>();
builder.Services.AddKafkaConsumerSettings(builder.Configuration, "Kafka");

builder.Services.AddSingleton<IValidateOptions<EmailSettings>, EmailSettingsValidator>();

builder.Services
    .AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateOnStart();

var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "EmailSender";
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? throw new NullReferenceException("OTEL_EXPORTER_OTLP_ENDPOINT");

builder.Services
    .AddCommonObservability(serviceName, otlpEndpoint);

var app = builder.Build();
app.Run();
