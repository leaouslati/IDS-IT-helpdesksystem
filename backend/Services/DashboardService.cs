using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DashboardService
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

            // Fetch tickets relevant to the 7-day trend window in one query
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
                RecentActivity = recentActivity,
                RecentTickets = recentTickets
            };
        }

        public async Task<ManagerDashboardDto> GetManagerDashboardAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);

            var teamOpenTickets = await _context.Tickets
                .CountAsync(t => t.TicketStatus.Name == "Open" || t.TicketStatus.Name == "In Progress");

            var resolvedThisWeek = await _context.Tickets
                .CountAsync(t => (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed")
                    && t.UpdatedAt.HasValue && t.UpdatedAt.Value >= weekStart);

            var resolvedForAvg = await _context.Tickets
                .Where(t => (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed")
                    && t.UpdatedAt.HasValue)
                .Select(t => new { t.CreatedAt, UpdatedAt = (DateTime)t.UpdatedAt! })
                .ToListAsync();

            var avgResolutionHours = resolvedForAvg.Count > 0
                ? Math.Round(resolvedForAvg.Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours), 2)
                : 0;

            var escalatedTickets = await _context.Tickets.CountAsync(t => t.IsEscalated);

            var agentRoleId = await _context.Roles
                .Where(r => r.Name == "Agent")
                .Select(r => r.Id)
                .FirstAsync();

            var agents = await _context.Users
                .Where(u => u.RoleId == agentRoleId && u.IsActive)
                .ToListAsync();

            var agentIds = agents.Select(a => a.Id).ToList();

            var agentTickets = await _context.Tickets
                .Where(t => t.AssignedToUserId.HasValue && agentIds.Contains(t.AssignedToUserId.Value))
                .Select(t => new { t.AssignedToUserId, StatusName = t.TicketStatus.Name, t.IsEscalated })
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

            var categoryBreakdown = await _context.Tickets
                .GroupBy(t => t.Category.Name)
                .Select(g => new CategoryBreakdownDto { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            var priorityBreakdown = await _context.Tickets
                .GroupBy(t => t.Priority.Name)
                .Select(g => new PriorityBreakdownDto { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            return new ManagerDashboardDto
            {
                TeamOpenTickets = teamOpenTickets,
                ResolvedThisWeek = resolvedThisWeek,
                AvgResolutionHours = avgResolutionHours,
                EscalatedTickets = escalatedTickets,
                AgentPerformance = agentPerformance,
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

            var pendingResponse = await _context.Tickets
                .CountAsync(t => t.AssignedToUserId == agentUserId
                    && t.TicketStatus.Name == "Pending");

            var resolvedThisWeek = await _context.Tickets
                .CountAsync(t => t.AssignedToUserId == agentUserId
                    && t.TicketStatus.Name == "Resolved"
                    && t.UpdatedAt.HasValue && t.UpdatedAt.Value >= weekStart);

            var myActiveTickets = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.TicketStatus)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.AssignedToUserId == agentUserId
                    && t.TicketStatus.Name != "Resolved"
                    && t.TicketStatus.Name != "Closed")
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
                PendingResponse = pendingResponse,
                ResolvedThisWeek = resolvedThisWeek,
                MyActiveTickets = myActiveTickets,
                RecentActivity = recentActivity
            };
        }

        public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeUserId)
        {
            var myOpenTickets = await _context.Tickets
                .CountAsync(t => t.CreatedByUserId == employeeUserId && t.TicketStatus.Name == "Open");

            var myResolvedTickets = await _context.Tickets
                .CountAsync(t => t.CreatedByUserId == employeeUserId
                    && (t.TicketStatus.Name == "Resolved" || t.TicketStatus.Name == "Closed"));

            var myPendingTickets = await _context.Tickets
                .CountAsync(t => t.CreatedByUserId == employeeUserId && t.TicketStatus.Name == "Pending");

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
                MyResolvedTickets = myResolvedTickets,
                MyPendingTickets = myPendingTickets,
                MyTotalTickets = myTotalTickets,
                MyRecentTickets = myRecentTickets,
                MyNotifications = myNotifications
            };
        }
    }
}
