namespace backend.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}