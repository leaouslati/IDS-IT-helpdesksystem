namespace backend.Application.DTOs
{
    public class RecentTicketDto
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public bool IsEscalated { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AgentPerformanceDto
    {
        public string AgentName { get; set; } = string.Empty;
        public int ResolvedTickets { get; set; }
        public int OpenTickets { get; set; }
        public int EscalatedTickets { get; set; }
    }

    public class CategoryBreakdownDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PriorityBreakdownDto
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TicketTrendDto
    {
        public string Date { get; set; } = string.Empty;
        public int Created { get; set; }
        public int Resolved { get; set; }
    }

    public class RecentActivityDto
    {
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime LoggedAt { get; set; }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AgentAvailabilityDto
    {
        public int UserId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public int OpenTickets { get; set; }
        public int ResolvedThisWeek { get; set; }
        public bool IsAvailable { get; set; }
    }
}
