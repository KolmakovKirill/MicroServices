using Minio;
using CommonShared.Infrastructure.DataStorage;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMinio(configureClient => configureClient
                                                .WithEndpoint("localhost:9010")
                                                .WithCredentials("miniouser", "admin123")
                                                .WithSSL(false)
                                                .Build());

builder.Services.AddDbContext<ServerDbContext>(options =>
{
    options.UseNpgsql(
        "Host=localhost;Port=5332;Database=myapp;Username=postgres;Password=password",
        npgsql => npgsql.MigrationsAssembly("CommonShared")
    );
});
builder.Services.AddScoped<EmailHandler>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHostedService<KafkaConsumerService<EmailHandler>>();

var app = builder.Build();
app.Run();
