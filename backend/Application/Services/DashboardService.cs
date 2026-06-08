using backend.Application.DTOs;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-6);

            var openTickets = await _context.Tickets
                .CountAsync(t => t.TicketStatus.Name == "Open");

            var pendingTickets = await _context.Tickets
                .CountAsync(t => t.TicketStatus.Name == "Pending");

            var resolvedToday = await _context.Tickets
                .CountAsync(t => t.TicketStatus.Name == "Resolved"
                    && t.UpdatedAt.HasValue && t.UpdatedAt.Value.Date == today);

            var criticalTickets = await _context.Tickets
                .CountAsync(t => t.Priority.Name == "Critical"
                    && t.TicketStatus.Name != "Resolved"
                    && t.TicketStatus.Name != "Closed");

            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalTickets = await _context.Tickets.CountAsync();
            var escalatedTickets = await _context.Tickets.CountAsync(t => t.IsEscalated);

            var trendTickets = await _context.Tickets
                .Where(t => t.CreatedAt >= sevenDaysAgo
                    || (t.UpdatedAt.HasValue && t.UpdatedAt.Value >= sevenDaysAgo && t.TicketStatus.Name == "Resolved"))
                .Select(t => new { t.CreatedAt, t.UpdatedAt, StatusName = t.TicketStatus.Name })
                .ToListAsync();

            var ticketTrend = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(i - 6))
                .Select(date => new TicketTrendDto
                {
                    Date = date.ToString("MMM dd"),
                    Created = trendTickets.Count(t => t.CreatedAt.Date == date),
                    Resolved = trendTickets.Count(t =>
                        t.StatusName == "Resolved"
                        && t.UpdatedAt.HasValue
                        && t.UpdatedAt.Value.Date == date)
                })
                .ToList();

            var categoryBreakdown = await _context.Tickets
                .GroupBy(t => t.Category.Name)
                .Select(g => new CategoryBreakdownDto { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            var priorityBreakdown = await _context.Tickets
                .GroupBy(t => t.Priority.Name)
                .Select(g => new PriorityBreakdownDto { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentActivity = await _context.ActivityLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.LoggedAt)
                .Take(10)
                .Select(a => new RecentActivityDto
                {
                    Action = a.Action,
                    Details = a.Details ?? string.Empty,
                    UserName = a.User.FirstName + " " + a.User.LastName,
                    LoggedAt = a.LoggedAt
                })
                .ToListAsync();

            var recentTickets = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new RecentTicketDto
                {
                    Id = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title = t.Title,
                    Category = t.Category.Name,
                    Priority = t.Priority.Name,
                    Status = t.TicketStatus.Name,
                    CreatedBy = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo = t.AssignedToUser != null
                        ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName
                        : null,
                    IsEscalated = t.IsEscalated,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new AdminDashboardDto
            {
                OpenTickets = openTickets,
                PendingTickets = pendingTickets,
                ResolvedToday = resolvedToday,
                CriticalTickets = criticalTickets,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalTickets = totalTickets,
                EscalatedTickets = escalatedTickets,
                TicketTrend = ticketTrend,
                CategoryBreakdown = categoryBreakdown,
                PriorityBreakdown = priorityBreakdown,
                RecentActivity = recentActivity,
                RecentTickets = recentTickets
            };
        }

        public async Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId)
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);

            var deptId = await _context.Users
                .Where(u => u.Id == managerId)
                .Select(u => u.DepartmentId)
                .FirstOrDefaultAsync();

            if (!deptId.HasValue)
                return new ManagerDashboardDto();

            IQueryable<Ticket> deptQuery = _context.Tickets.Where(t => t.DepartmentId == deptId.Value);

            var teamOpenTickets = await deptQuery
                .CountAsync(t => t.TicketStatus.Name == "Open" || t.TicketStatus.Name == "In Progress");

            var unassignedTickets = await deptQuery
                .CountAsync(t => t.AssignedToUserId == null);

            var resolvedThisWeek = await deptQuery
                .CountAsync(t => (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed")
                    && t.UpdatedAt.HasValue && t.UpdatedAt.Value >= weekStart);

            var resolvedForAvg = await deptQuery
                .Where(t => (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed")
                    && t.UpdatedAt.HasValue)
                .Select(t => new { t.CreatedAt, UpdatedAt = (DateTime)t.UpdatedAt! })
                .ToListAsync();

            var avgResolutionHours = resolvedForAvg.Count > 0
                ? Math.Round(resolvedForAvg.Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours), 2)
                : 0;

            var escalatedTickets = await deptQuery.CountAsync(t => t.IsEscalated);

            var agentRoleId = await _context.Roles
                .Where(r => r.Name == "Agent")
                .Select(r => r.Id)
                .FirstAsync();

            var agents = await _context.Users
                .Where(u => u.RoleId == agentRoleId && u.IsActive && u.DepartmentId == deptId.Value)
                .ToListAsync();

            var agentIds = agents.Select(a => a.Id).ToList();

            var agentTickets = await _context.Tickets
                .Where(t => t.AssignedToUserId.HasValue && agentIds.Contains(t.AssignedToUserId.Value))
                .Select(t => new { t.AssignedToUserId, StatusName = t.TicketStatus.Name, t.IsEscalated, t.UpdatedAt })
                .ToListAsync();

            var agentPerformance = agents.Select(agent => new AgentPerformanceDto
            {
                AgentName = agent.FirstName + " " + agent.LastName,
                ResolvedTickets = agentTickets.Count(t =>
                    t.AssignedToUserId == agent.Id
                    && (t.StatusName == "Resolved" || t.StatusName == "Closed")),
                OpenTickets = agentTickets.Count(t =>
                    t.AssignedToUserId == agent.Id
                    && (t.StatusName == "Open" || t.StatusName == "In Progress")),
                EscalatedTickets = agentTickets.Count(t =>
                    t.AssignedToUserId == agent.Id && t.IsEscalated)
            }).ToList();

            var agentAvailability = agents.Select(agent =>
            {
                var openCount = agentTickets.Count(t =>
                    t.AssignedToUserId == agent.Id
                    && (t.StatusName == "Open" || t.StatusName == "In Progress"));
                var resolvedCount = agentTickets.Count(t =>
                    t.AssignedToUserId == agent.Id
                    && (t.StatusName == "Resolved" || t.StatusName == "Closed")
                    && t.UpdatedAt.HasValue && t.UpdatedAt.Value >= weekStart);
                return new AgentAvailabilityDto
                {
                    UserId = agent.Id,
                    AgentName = agent.FirstName + " " + agent.LastName,
                    OpenTickets = openCount,
                    ResolvedThisWeek = resolvedCount,
                    IsAvailable = openCount < 5
                };
            }).ToList();

            var unassignedTicketsList = await deptQuery
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Where(t => t.AssignedToUserId == null)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new RecentTicketDto
                {
                    Id = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title = t.Title,
                    Category = t.Category.Name,
                    Priority = t.Priority.Name,
                    Status = t.TicketStatus.Name,
                    CreatedBy = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo = null,
                    IsEscalated = t.IsEscalated,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            var recentTickets = await deptQuery
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new RecentTicketDto
                {
                    Id = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title = t.Title,
                    Category = t.Category.Name,
                    Priority = t.Priority.Name,
                    Status = t.TicketStatus.Name,
                    CreatedBy = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo = t.AssignedToUser != null
                        ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName
                        : null,
                    IsEscalated = t.IsEscalated,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            var categoryBreakdown = await deptQuery
                .GroupBy(t => t.Category.Name)
                .Select(g => new CategoryBreakdownDto { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            var priorityBreakdown = await deptQuery
                .GroupBy(t => t.Priority.Name)
                .Select(g => new PriorityBreakdownDto { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            return new ManagerDashboardDto
            {
                TeamOpenTickets = teamOpenTickets,
                UnassignedTickets = unassignedTickets,
                ResolvedThisWeek = resolvedThisWeek,
                AvgResolutionHours = avgResolutionHours,
                EscalatedTickets = escalatedTickets,
                AgentPerformance = agentPerformance,
                AgentAvailability = agentAvailability,
                UnassignedTicketsList = unassignedTicketsList,
                RecentTickets = recentTickets,
                CategoryBreakdown = categoryBreakdown,
                PriorityBreakdown = priorityBreakdown
            };
        }

        public async Task<AgentDashboardDto> GetAgentDashboardAsync(int agentUserId)
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);

            var assignedToMe = await _context.Tickets
                .CountAsync(t => t.AssignedToUserId == agentUserId
                    && t.TicketStatus.Name != "Resolved"
                    && t.TicketStatus.Name != "Closed");

            var resolvedToday = await _context.Tickets
                .CountAsync(t => t.AssignedToUserId == agentUserId
                    && t.TicketStatus.Name == "Resolved"
                    && t.UpdatedAt.HasValue && t.UpdatedAt.Value.Date == today);

            var inProgress = await _context.Tickets
                .CountAsync(t => t.AssignedToUserId == agentUserId
                    && t.TicketStatus.Name == "In Progress");

            var resolvedThisWeek = await _context.Tickets
                .CountAsync(t => t.AssignedToUserId == agentUserId
                    && t.TicketStatus.Name == "Resolved"
                    && t.UpdatedAt.HasValue && t.UpdatedAt.Value >= weekStart);

            var escalatedCount = await _context.Tickets
                .CountAsync(t => t.AssignedToUserId == agentUserId && t.IsEscalated);

            var myActiveTickets = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.AssignedToUserId == agentUserId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new RecentTicketDto
                {
                    Id = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title = t.Title,
                    Category = t.Category.Name,
                    Priority = t.Priority.Name,
                    Status = t.TicketStatus.Name,
                    CreatedBy = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo = t.AssignedToUser != null
                        ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName
                        : null,
                    IsEscalated = t.IsEscalated,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            var recentActivity = await _context.ActivityLogs
                .Include(a => a.User)
                .Where(a => a.UserId == agentUserId)
                .OrderByDescending(a => a.LoggedAt)
                .Take(10)
                .Select(a => new RecentActivityDto
                {
                    Action = a.Action,
                    Details = a.Details ?? string.Empty,
                    UserName = a.User.FirstName + " " + a.User.LastName,
                    LoggedAt = a.LoggedAt
                })
                .ToListAsync();

            return new AgentDashboardDto
            {
                AssignedToMe = assignedToMe,
                ResolvedToday = resolvedToday,
                InProgress = inProgress,
                ResolvedThisWeek = resolvedThisWeek,
                EscalatedCount = escalatedCount,
                MyTickets = myActiveTickets,
                RecentActivity = recentActivity
            };
        }

        public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeUserId)
        {
            var myOpenTickets = await _context.Tickets
                .CountAsync(t => t.CreatedByUserId == employeeUserId && t.TicketStatus.Name == "Open");

            var myInProgressTickets = await _context.Tickets
                .CountAsync(t => t.CreatedByUserId == employeeUserId && t.TicketStatus.Name == "In Progress");

            var myResolvedTickets = await _context.Tickets
                .CountAsync(t => t.CreatedByUserId == employeeUserId
                    && (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed"));

            var myTotalTickets = await _context.Tickets
                .CountAsync(t => t.CreatedByUserId == employeeUserId);

            var myRecentTickets = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.CreatedByUserId == employeeUserId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new RecentTicketDto
                {
                    Id = t.Id,
                    ReferenceNumber = t.ReferenceNumber,
                    Title = t.Title,
                    Category = t.Category.Name,
                    Priority = t.Priority.Name,
                    Status = t.TicketStatus.Name,
                    CreatedBy = t.CreatedByUser.FirstName + " " + t.CreatedByUser.LastName,
                    AssignedTo = t.AssignedToUser != null
                        ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName
                        : null,
                    IsEscalated = t.IsEscalated,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            var myNotifications = await _context.Notifications
                .Where(n => n.UserId == employeeUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return new EmployeeDashboardDto
            {
                MyOpenTickets = myOpenTickets,
                MyInProgressTickets = myInProgressTickets,
                MyResolvedTickets = myResolvedTickets,
                MyTotalTickets = myTotalTickets,
                MyRecentTickets = myRecentTickets,
                MyNotifications = myNotifications
            };
        }
    }
}
