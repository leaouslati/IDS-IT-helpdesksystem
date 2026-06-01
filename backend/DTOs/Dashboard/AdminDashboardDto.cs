namespace backend.DTOs
{
    public class AdminDashboardDto
    {
        public int OpenTickets { get; set; }
        public int PendingTickets { get; set; }
        public int ResolvedToday { get; set; }
        public int CriticalTickets { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalTickets { get; set; }
        public int EscalatedTickets { get; set; }
        public List<TicketTrendDto> TicketTrend { get; set; } = new();
        public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public List<RecentTicketDto> RecentTickets { get; set; } = new();
    }
}
