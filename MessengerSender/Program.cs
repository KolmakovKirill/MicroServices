using Minio;
using CommonShared.Infrastructure.DataStorage;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<MessengerHandler>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddLogging();
builder.Services.AddHostedService<KafkaConsumerService<MessengerHandler>>();

var app = builder.Build();
app.Run();
