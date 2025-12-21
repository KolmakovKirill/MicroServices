using CommonShared.Configuration;
using Minio;
using CommonShared.Infrastructure.DataStorage;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PushSender.Configuration;
using PushSender.Infrastructure.Handlers;
using PushSender.Services;

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
builder.Services.AddScoped<PushNotificationHandler>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddLogging();
builder.Services.AddHostedService<KafkaConsumerService<PushNotificationHandler>>();
builder.Services.AddKafkaConsumerSettings(builder.Configuration, "Kafka");
builder.Services.AddSingleton<IFirebaseAppProvider, FirebaseAppProvider>();

builder.Services.AddSingleton<IValidateOptions<FirebaseSettings>, FirebaseSettingsValidator>();

builder.Services
    .AddOptions<FirebaseSettings>()
    .Bind(builder.Configuration.GetSection("FirebaseSettings"))
    .ValidateOnStart();

var app = builder.Build();
app.Run();
