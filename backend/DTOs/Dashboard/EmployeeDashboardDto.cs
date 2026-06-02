namespace backend.DTOs
{
    public class EmployeeDashboardDto
    {
        public int MyOpenTickets { get; set; }
        public int MyInProgressTickets { get; set; }
        public int MyResolvedTickets { get; set; }
        public int MyTotalTickets { get; set; }
        public List<RecentTicketDto> MyRecentTickets { get; set; } = new();
        public List<NotificationDto> MyNotifications { get; set; } = new();
    }
}
