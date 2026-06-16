using System.ComponentModel.DataAnnotations;

namespace backend.Application.DTOs
{
    public class LogHoursDto
    {
        [Range(0.25, 24.0, ErrorMessage = "Hours worked must be between 0.25 and 24.")]
        public decimal HoursWorked { get; set; }

        public DateTime LogDate { get; set; } = DateTime.UtcNow.Date;

        public string? Notes { get; set; }
    }

    public class HoursLogResponseDto
    {
        public int TicketHoursLogId { get; set; }
        public int TicketId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public DateTime LogDate { get; set; }
        public string? Notes { get; set; }
    }
}
