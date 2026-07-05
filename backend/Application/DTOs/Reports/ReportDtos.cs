namespace backend.Application.DTOs.Reports
{
    public class DailyVolumeDto
    {
        public string Date { get; set; } = string.Empty;
        public int Opened { get; set; }
        public int Closed { get; set; }
    }

    public class TicketVolumeReportDto
    {
        public List<DailyVolumeDto> Daily { get; set; } = new();
        public int TotalOpened { get; set; }
        public int TotalClosed { get; set; }
        public int NetChange { get; set; }
    }

    public class ResolutionByGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public double AvgHours { get; set; }
        public int Count { get; set; }
    }

    public class ResolutionTimeReportDto
    {
        public double AvgHoursOverall { get; set; }
        public int TotalResolved { get; set; }
        public List<ResolutionByGroupDto> ByCategory { get; set; } = new();
        public List<ResolutionByGroupDto> ByPriority { get; set; } = new();
    }

    public class EmployeeReportRowDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int Open { get; set; }
        public int InProgress { get; set; }
        public int Pending { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
    }

    public class SummaryReportDto
    {
        public TicketVolumeReportDto Volume { get; set; } = new();
        public ResolutionTimeReportDto ResolutionTime { get; set; } = new();
        public List<EmployeeReportRowDto> ByEmployee { get; set; } = new();
    }
}
