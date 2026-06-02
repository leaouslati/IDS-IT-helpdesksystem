namespace backend.DTOs
{
    public class ManagerDashboardDto
    {
        public int TeamOpenTickets { get; set; }
        public int UnassignedTickets { get; set; }
        public int ResolvedThisWeek { get; set; }
        public double AvgResolutionHours { get; set; }
        public int EscalatedTickets { get; set; }
        public List<AgentPerformanceDto> AgentPerformance { get; set; } = new();
        public List<AgentAvailabilityDto> AgentAvailability { get; set; } = new();
        public List<RecentTicketDto> UnassignedTicketsList { get; set; } = new();
        public List<RecentTicketDto> RecentTickets { get; set; } = new();
        public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
        public List<PriorityBreakdownDto> PriorityBreakdown { get; set; } = new();
    }
}
