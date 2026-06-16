using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── Admin ────────────────────────────────────────────────────────────

        public async Task<int> CountTicketsByStatusNameAsync(string statusName) =>
            await _context.Tickets.CountAsync(t => t.TicketStatus.Name == statusName);

        public async Task<int> CountTicketsResolvedOnDateAsync(DateTime date) =>
            await _context.Tickets.CountAsync(t =>
                t.TicketStatus.Name == "Resolved" &&
                t.UpdatedAt.HasValue && t.UpdatedAt.Value.Date == date.Date);

        public async Task<int> CountActiveCriticalTicketsAsync() =>
            await _context.Tickets.CountAsync(t =>
                t.Priority.Name == "Critical" &&
                t.TicketStatus.Name != "Resolved");

        public async Task<int> CountUsersAsync(bool? isActive = null) =>
            isActive.HasValue
                ? await _context.Users.CountAsync(u => u.IsActive == isActive.Value)
                : await _context.Users.CountAsync();

        public async Task<int> CountAllTicketsAsync() =>
            await _context.Tickets.CountAsync();

        public async Task<int> CountEscalatedTicketsAsync() =>
            await _context.Tickets.CountAsync(t => t.IsEscalated);

        public async Task<List<TicketTrendDto>> GetTicketTrendAsync(DateTime from)
        {
            var today = DateTime.UtcNow.Date;
            var raw = await _context.Tickets
                .Where(t => t.CreatedAt >= from ||
                    (t.UpdatedAt.HasValue && t.UpdatedAt.Value >= from && t.TicketStatus.Name == "Resolved"))
                .Select(t => new { t.CreatedAt, t.UpdatedAt, StatusName = t.TicketStatus.Name })
                .ToListAsync();

            return Enumerable.Range(0, 7)
                .Select(i => today.AddDays(i - 6))
                .Select(date => new TicketTrendDto
                {
                    Date = date.ToString("MMM dd"),
                    Created = raw.Count(t => t.CreatedAt.Date == date),
                    Resolved = raw.Count(t =>
                        t.StatusName == "Resolved" &&
                        t.UpdatedAt.HasValue &&
                        t.UpdatedAt.Value.Date == date)
                })
                .ToList();
        }

        public async Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(int? deptId = null)
        {
            var query = _context.Tickets.AsQueryable();
            if (deptId.HasValue) query = query.Where(t => t.DepartmentId == deptId.Value);
            return await query
                .GroupBy(t => t.Category.Name)
                .Select(g => new CategoryBreakdownDto { Category = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        public async Task<List<PriorityBreakdownDto>> GetPriorityBreakdownAsync(int? deptId = null)
        {
            var query = _context.Tickets.AsQueryable();
            if (deptId.HasValue) query = query.Where(t => t.DepartmentId == deptId.Value);
            return await query
                .GroupBy(t => t.Priority.Name)
                .Select(g => new PriorityBreakdownDto { Priority = g.Key, Count = g.Count() })
                .ToListAsync();
        }

        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int? userId = null, int take = 10)
        {
            var query = _context.ActivityLogs.Include(a => a.User).AsQueryable();
            if (userId.HasValue) query = query.Where(a => a.UserId == userId.Value);
            return await query
                .OrderByDescending(a => a.LoggedAt)
                .Take(take)
                .Select(a => new RecentActivityDto
                {
                    Action = a.Action,
                    Details = a.Details ?? string.Empty,
                    UserName = a.User.FirstName + " " + a.User.LastName,
                    LoggedAt = a.LoggedAt
                })
                .ToListAsync();
        }

        public async Task<List<RecentTicketDto>> GetRecentTicketsAsync(
            int take = 10, int? deptId = null, int? assignedToUserId = null, int? createdByUserId = null)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .AsQueryable();

            if (deptId.HasValue)          query = query.Where(t => t.DepartmentId == deptId.Value);
            if (assignedToUserId.HasValue) query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);
            if (createdByUserId.HasValue)  query = query.Where(t => t.CreatedByUserId == createdByUserId.Value);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Take(take)
                .Select(t => new RecentTicketDto
                {
                    Id              = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title           = t.Title,
                    Category        = t.Category.Name,
                    Priority        = t.Priority.Name,
                    Status          = t.TicketStatus.Name,
                    CreatedBy       = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo      = t.AssignedToUser != null
                        ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName : null,
                    IsEscalated = t.IsEscalated,
                    CreatedAt   = t.CreatedAt
                })
                .ToListAsync();
        }

        // ── Manager ──────────────────────────────────────────────────────────

        public async Task<int?> GetUserDepartmentIdAsync(int userId) =>
            await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.DepartmentId)
                .FirstOrDefaultAsync();

        public async Task<int> CountDeptTicketsByStatusNamesAsync(int deptId, IEnumerable<string> statusNames) =>
            await _context.Tickets.CountAsync(t =>
                t.DepartmentId == deptId && statusNames.Contains(t.TicketStatus.Name));

        public async Task<int> CountDeptUnassignedTicketsAsync(int deptId) =>
            await _context.Tickets.CountAsync(t =>
                t.DepartmentId == deptId && t.AssignedToUserId == null);

        public async Task<int> CountDeptEscalatedTicketsAsync(int deptId) =>
            await _context.Tickets.CountAsync(t =>
                t.DepartmentId == deptId && t.IsEscalated);

        public async Task<int> CountDeptTicketsResolvedSinceAsync(int deptId, DateTime since) =>
            await _context.Tickets.CountAsync(t =>
                t.DepartmentId == deptId &&
                t.TicketStatus.Name == "Resolved" &&
                t.UpdatedAt.HasValue && t.UpdatedAt.Value >= since);

        public async Task<List<(DateTime CreatedAt, DateTime UpdatedAt)>> GetResolvedTicketTimesAsync(int deptId) =>
            (await _context.Tickets
                .Where(t => t.DepartmentId == deptId &&
                    t.TicketStatus.Name == "Resolved" &&
                    t.UpdatedAt.HasValue)
                .Select(t => new { t.CreatedAt, UpdatedAt = (DateTime)t.UpdatedAt! })
                .ToListAsync())
                .Select(x => (x.CreatedAt, x.UpdatedAt))
                .ToList();

        public async Task<int> GetAgentRoleIdAsync() =>
            await _context.Roles
                .Where(r => r.Name == "Agent")
                .Select(r => r.Id)
                .FirstAsync();

        public async Task<List<User>> GetAgentsInDepartmentAsync(int deptId, int agentRoleId) =>
            await _context.Users
                .Where(u => u.RoleId == agentRoleId && u.IsActive && u.DepartmentId == deptId)
                .ToListAsync();

        public async Task<List<AgentTicketData>> GetAgentTicketDataAsync(List<int> agentIds) =>
            (await _context.Tickets
                .Where(t => t.AssignedToUserId.HasValue && agentIds.Contains(t.AssignedToUserId.Value))
                .Select(t => new
                {
                    AgentId    = t.AssignedToUserId!.Value,
                    StatusName = t.TicketStatus.Name,
                    t.IsEscalated,
                    t.UpdatedAt
                })
                .ToListAsync())
                .Select(x => new AgentTicketData(x.AgentId, x.StatusName, x.IsEscalated, x.UpdatedAt))
                .ToList();

        public async Task<List<RecentTicketDto>> GetUnassignedTicketsInDeptAsync(int deptId, int take = 20) =>
            await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Where(t => t.DepartmentId == deptId && t.AssignedToUserId == null)
                .OrderByDescending(t => t.CreatedAt)
                .Take(take)
                .Select(t => new RecentTicketDto
                {
                    Id              = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title           = t.Title,
                    Category        = t.Category.Name,
                    Priority        = t.Priority.Name,
                    Status          = t.TicketStatus.Name,
                    CreatedBy       = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo      = null,
                    IsEscalated     = t.IsEscalated,
                    CreatedAt       = t.CreatedAt
                })
                .ToListAsync();

        // ── Agent ─────────────────────────────────────────────────────────────

        public async Task<int> CountAgentActiveTicketsAsync(int agentId) =>
            await _context.Tickets.CountAsync(t =>
                t.AssignedToUserId == agentId &&
                t.TicketStatus.Name != "Resolved");

        public async Task<int> CountAgentTicketsByStatusNameAsync(int agentId, string statusName) =>
            await _context.Tickets.CountAsync(t =>
                t.AssignedToUserId == agentId && t.TicketStatus.Name == statusName);

        public async Task<int> CountAgentTicketsResolvedTodayAsync(int agentId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Tickets.CountAsync(t =>
                t.AssignedToUserId == agentId &&
                t.TicketStatus.Name == "Resolved" &&
                t.UpdatedAt.HasValue && t.UpdatedAt.Value.Date == today);
        }

        public async Task<int> CountAgentTicketsResolvedSinceAsync(int agentId, DateTime since) =>
            await _context.Tickets.CountAsync(t =>
                t.AssignedToUserId == agentId &&
                t.TicketStatus.Name == "Resolved" &&
                t.UpdatedAt.HasValue && t.UpdatedAt.Value >= since);

        public async Task<int> CountAgentEscalatedTicketsAsync(int agentId) =>
            await _context.Tickets.CountAsync(t =>
                t.AssignedToUserId == agentId && t.IsEscalated);

        // ── Employee ──────────────────────────────────────────────────────────

        public async Task<int> CountEmployeeTicketsByStatusNameAsync(int employeeId, string statusName) =>
            await _context.Tickets.CountAsync(t =>
                t.CreatedByUserId == employeeId && t.TicketStatus.Name == statusName);

        public async Task<int> CountEmployeeTicketsByStatusNamesAsync(int employeeId, IEnumerable<string> statusNames) =>
            await _context.Tickets.CountAsync(t =>
                t.CreatedByUserId == employeeId && statusNames.Contains(t.TicketStatus.Name));

        public async Task<int> CountAllEmployeeTicketsAsync(int employeeId) =>
            await _context.Tickets.CountAsync(t => t.CreatedByUserId == employeeId);

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId) =>
            await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id        = n.Id,
                    Message   = n.Message,
                    IsRead    = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
    }
}
