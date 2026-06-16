namespace backend.Domain.Entities
{
    public class TicketHoursLog
    {
        public int TicketHoursLogId { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public int AgentId { get; set; }
        public User Agent { get; set; } = null!;
        public decimal HoursWorked { get; set; }
        public DateTime LogDate { get; set; }
        public string? Notes { get; set; }
    }
}
