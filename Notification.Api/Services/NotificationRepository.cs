using Notification.Shared;
using Npgsql;
using Dapper;

namespace Notification.Api.Services
{
    public class NotificationRepository
    {
        private readonly string _connStr;
        public NotificationRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("Default") ?? "Host=localhost;Database=notifications;Username=notif_user;Password=notif_pass";
        }

        public async Task SaveAsync(NotificationRequest req, NotificationStatus status)
        {
            using var conn = new NpgsqlConnection(_connStr);
            await conn.ExecuteAsync(@"INSERT INTO notifications (id, channel, recipient, subject, message, createdat, status) VALUES (@Id, @Channel, @Recipient, @Subject, @Message, @CreatedAt, @Status)",
                new {
                    req.Id,
                    Channel = req.Channel.ToString(),
                    req.Recipient,
                    req.Subject,
                    req.Message,
                    req.CreatedAt,
                    Status = status.ToString()
                });
        }

        public async Task<IEnumerable<NotificationRequest>> GetHistoryAsync()
        {
            using var conn = new NpgsqlConnection(_connStr);
            var results = await conn.QueryAsync<NotificationRequest>(@"SELECT id, recipient, subject, message, createdat, channel FROM notifications");
            return results;
        }
    }
}
