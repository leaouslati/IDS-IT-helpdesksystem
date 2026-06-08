using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domain.Entities
{
    public class TicketComment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; } = null!;
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public bool IsEscalationComment { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
