namespace backend.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int? TicketId { get; set; }
        public Ticket? Ticket { get; set; }
        // NotificationType constants: TicketCreated, TicketAssigned, TicketEscalated, TicketClosed, CommentAdded, AttachmentAdded
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
