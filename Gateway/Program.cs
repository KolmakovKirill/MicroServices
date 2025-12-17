using Minio;
using CommonShared.Infrastructure.DataStorage;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

await KafkaTopicInitializer.EnsureTopicsAsync(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var mediaService = scope.ServiceProvider.GetRequiredService<MediaService>();
    await mediaService.InitializeAsync();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServerDbContext>();
    db.Database.Migrate(); 
}


app.MapControllers();
app.Run();
