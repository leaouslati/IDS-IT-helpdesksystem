namespace backend.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public int PriorityId { get; set; }
        public Priority Priority { get; set; } = null!;
        public int TicketStatusId { get; set; }
        public TicketStatus TicketStatus { get; set; } = null!;
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public int? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public bool IsEscalated { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    }
}