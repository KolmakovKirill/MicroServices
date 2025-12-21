using CommonShared.Configuration;
using Minio;
using CommonShared.Infrastructure.DataStorage;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmsSender.Configuration;
using SmsSender.Infrastructure.Handlers;

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
builder.Services.AddScoped<SmsNotificationHandler>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddLogging();
builder.Services.AddHostedService<KafkaConsumerService<SmsNotificationHandler>>();
builder.Services.AddKafkaConsumerSettings(builder.Configuration, "Kafka");

builder.Services.AddSingleton<IValidateOptions<SmsSettings>, SmsSettingsValidator>();

builder.Services
    .AddOptions<SmsSettings>()
    .Bind(builder.Configuration.GetSection("SmsSettings"))
    .ValidateOnStart();


var app = builder.Build();
app.Run();
