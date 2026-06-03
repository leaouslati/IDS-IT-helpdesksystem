namespace backend.Application.DTOs
{
    public class AgentDashboardDto
    {
        public int AssignedToMe { get; set; }
        public int ResolvedToday { get; set; }
        public int PendingResponse { get; set; }
        public int ResolvedThisWeek { get; set; }
        public int EscalatedCount { get; set; }
        public List<RecentTicketDto> MyTickets { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
    }
}
