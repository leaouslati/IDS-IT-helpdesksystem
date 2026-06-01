namespace backend.DTOs
{
    public class AgentDashboardDto
    {
        public int AssignedToMe { get; set; }
        public int ResolvedToday { get; set; }
        public int PendingResponse { get; set; }
        public int ResolvedThisWeek { get; set; }
        public List<RecentTicketDto> MyActiveTickets { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
    }
}
