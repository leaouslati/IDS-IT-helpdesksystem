using backend.Application.DTOs.Reports;
using backend.Application.Interfaces;

namespace backend.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<TicketVolumeReportDto> GetTicketVolumeAsync(DateTime from, DateTime to, int? departmentId = null)
        {
            var daily       = await _repo.GetDailyVolumeAsync(from, to, departmentId);
            int totalOpened = daily.Sum(d => d.Opened);
            int totalClosed = daily.Sum(d => d.Closed);

            return new TicketVolumeReportDto
            {
                Daily        = daily,
                TotalOpened  = totalOpened,
                TotalClosed  = totalClosed,
                NetChange    = totalOpened - totalClosed
            };
        }

        public async Task<ResolutionTimeReportDto> GetResolutionTimeAsync(DateTime from, DateTime to, int? departmentId = null)
        {
            var tickets = await _repo.GetResolvedTicketTimesAsync(from, to, departmentId);

            static double AvgHours(IEnumerable<(DateTime c, DateTime e, string cat, string pri)> items) =>
                items.Any() ? Math.Round(items.Average(x => (x.e - x.c).TotalHours), 1) : 0;

            return new ResolutionTimeReportDto
            {
                TotalResolved   = tickets.Count,
                AvgHoursOverall = AvgHours(tickets),
                ByCategory = tickets
                    .GroupBy(t => t.Category)
                    .Select(g => new ResolutionByGroupDto
                    {
                        Name     = g.Key,
                        Count    = g.Count(),
                        AvgHours = AvgHours(g)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                ByPriority = tickets
                    .GroupBy(t => t.Priority)
                    .Select(g => new ResolutionByGroupDto
                    {
                        Name     = g.Key,
                        Count    = g.Count(),
                        AvgHours = AvgHours(g)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };
        }

        public Task<List<EmployeeReportRowDto>> GetByEmployeeAsync(DateTime from, DateTime to, int? departmentId = null) =>
            _repo.GetEmployeeBreakdownAsync(from, to, departmentId);

        public async Task<SummaryReportDto> GetSummaryAsync(DateTime from, DateTime to, int? departmentId = null)
        {
            // EF Core DbContext is not thread-safe — queries must run sequentially on the same context.
            var volume         = await GetTicketVolumeAsync(from, to, departmentId);
            var resolutionTime = await GetResolutionTimeAsync(from, to, departmentId);
            var employees      = await GetByEmployeeAsync(from, to, departmentId);

            return new SummaryReportDto
            {
                Volume         = volume,
                ResolutionTime = resolutionTime,
                ByEmployee     = employees
            };
        }
    }
}
