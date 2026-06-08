namespace backend.Domain.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int? TicketId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}
