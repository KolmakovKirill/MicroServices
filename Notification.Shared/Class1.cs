namespace Notification.Shared
{
    public enum NotificationChannel
    {
        Email,
        Sms,
        Push
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed
    }

    public class NotificationRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public NotificationChannel Channel { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NotificationResult
    {
        public Guid RequestId { get; set; }
        public NotificationStatus Status { get; set; }
        public string? Error { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
