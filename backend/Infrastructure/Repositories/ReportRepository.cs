using backend.Application.DTOs.Reports;
using backend.Application.Interfaces;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DailyVolumeDto>> GetDailyVolumeAsync(DateTime from, DateTime to, int? departmentId = null)
        {
            var fromUtc     = from.Date;
            var toExclusive = to.Date.AddDays(1);

            var openedDates = await _context.Tickets
                .Where(t => t.CreatedAt >= fromUtc && t.CreatedAt < toExclusive
                    && (!departmentId.HasValue || t.DepartmentId == departmentId.Value))
                .Select(t => t.CreatedAt)
                .ToListAsync();

            var closedDates = await _context.Tickets
                .Where(t =>
                    (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed") &&
                    t.UpdatedAt.HasValue &&
                    t.UpdatedAt.Value >= fromUtc &&
                    t.UpdatedAt.Value < toExclusive
                    && (!departmentId.HasValue || t.DepartmentId == departmentId.Value))
                .Select(t => t.UpdatedAt!.Value)
                .ToListAsync();

            int totalDays = (int)(to.Date - from.Date).TotalDays + 1;
            return Enumerable.Range(0, totalDays)
                .Select(i =>
                {
                    var date = fromUtc.AddDays(i);
                    return new DailyVolumeDto
                    {
                        Date   = date.ToString("MMM dd"),
                        Opened = openedDates.Count(d => d.Date == date),
                        Closed = closedDates.Count(d => d.Date == date)
                    };
                })
                .ToList();
        }

        public async Task<List<(DateTime CreatedAt, DateTime ClosedAt, string Category, string Priority)>> GetResolvedTicketTimesAsync(
            DateTime from, DateTime to, int? departmentId = null)
        {
            var fromUtc     = from.Date;
            var toExclusive = to.Date.AddDays(1);

            return (await _context.Tickets
                .Where(t =>
                    (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed") &&
                    t.UpdatedAt.HasValue &&
                    t.UpdatedAt.Value >= fromUtc &&
                    t.UpdatedAt.Value < toExclusive
                    && (!departmentId.HasValue || t.DepartmentId == departmentId.Value))
                .Select(t => new
                {
                    t.CreatedAt,
                    ClosedAt = t.UpdatedAt!.Value,
                    Category = t.Category.Name,
                    Priority = t.Priority.Name
                })
                .ToListAsync())
                .Select(x => (x.CreatedAt, x.ClosedAt, x.Category, x.Priority))
                .ToList();
        }

        public async Task<List<EmployeeReportRowDto>> GetEmployeeBreakdownAsync(
            DateTime from, DateTime to, int? departmentId = null)
        {
            var fromUtc     = from.Date;
            var toExclusive = to.Date.AddDays(1);

            var employeeRoleId = await _context.Roles.Where(r => r.Name == "Employee").Select(r => r.Id).FirstAsync();
            var agentRoleId    = await _context.Roles.Where(r => r.Name == "Agent").Select(r => r.Id).FirstAsync();

            var usersQuery = _context.Users
                .Where(u => u.IsActive && (u.RoleId == employeeRoleId || u.RoleId == agentRoleId));
            if (departmentId.HasValue)
                usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);

            var users = await usersQuery
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName, u.RoleId })
                .ToListAsync();

            if (users.Count == 0) return new List<EmployeeReportRowDto>();

            var tickets = await _context.Tickets
                .Where(t => t.CreatedAt >= fromUtc && t.CreatedAt < toExclusive
                    && (!departmentId.HasValue || t.DepartmentId == departmentId.Value))
                .Select(t => new
                {
                    t.CreatedByUserId,
                    t.AssignedToUserId,
                    StatusName = t.TicketStatus.Name
                })
                .ToListAsync();

            return users
                .Select(u =>
                {
                    var isEmployee = u.RoleId == employeeRoleId;
                    var myTickets  = isEmployee
                        ? tickets.Where(t => t.CreatedByUserId  == u.Id).ToList()
                        : tickets.Where(t => t.AssignedToUserId == u.Id).ToList();

                    return new EmployeeReportRowDto
                    {
                        UserId       = u.Id,
                        Name         = u.Name,
                        Role         = isEmployee ? "Employee" : "Agent",
                        TotalTickets = myTickets.Count,
                        Open         = myTickets.Count(t => t.StatusName == "Open"),
                        InProgress   = myTickets.Count(t => t.StatusName == "In Progress"),
                        Pending      = myTickets.Count(t => t.StatusName == "Pending"),
                        Resolved     = myTickets.Count(t => t.StatusName == "Resolved"),
                        Closed       = myTickets.Count(t => t.StatusName == "Closed"),
                    };
                })
                .OrderByDescending(r => r.TotalTickets)
                .ToList();
        }
    }
}
