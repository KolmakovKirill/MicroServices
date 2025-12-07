using Minio;
using microservices_project.Infrastructure.DataStorage;
using microservices_project.Infrastructure.DataStorage.Services;
using microservices_project.Infrastructure.Messaging.Services;
// using microservices_project.Infrastructure.DataStorage.Profiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMinio(configureClient => configureClient
                                                .WithEndpoint("localhost:9010")
                                                .WithCredentials("miniouser", "admin123")
                                                .WithSSL(false)
                                                .Build());

builder.Services.AddDbContext<ServerDbContext>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddAutoMapper(typeof(AppMappingProfile));

await KafkaTopicInitializer.EnsureTopicsAsync(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var mediaService = scope.ServiceProvider.GetRequiredService<MediaService>();
    await mediaService.InitializeAsync();
}

app.MapControllers();
app.Run();
