using backend.Application.DTOs;
using backend.Application.Interfaces;

namespace backend.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;

        public DashboardService(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync(int days = 30)
        {
            var today      = DateTime.UtcNow.Date;
            var windowStart = DateTime.UtcNow.AddDays(-days);
            var sevenDaysAgo = today.AddDays(-6);

            var openTickets       = await _repo.CountTicketsByStatusNameAsync("Open", windowStart);
            var pendingTickets    = await _repo.CountTicketsByStatusNameAsync("Pending", windowStart);
            var resolvedToday     = await _repo.CountTicketsResolvedOnDateAsync(today);
            var criticalTickets   = await _repo.CountActiveCriticalTicketsAsync();
            var totalUsers        = await _repo.CountUsersAsync();
            var activeUsers       = await _repo.CountUsersAsync(true);
            var totalTickets      = await _repo.CountAllTicketsAsync(windowStart);
            var escalatedTickets  = await _repo.CountEscalatedTicketsAsync(windowStart);
            var ticketTrend       = await _repo.GetTicketTrendAsync(sevenDaysAgo);
            var categoryBreakdown = await _repo.GetCategoryBreakdownAsync(null, windowStart);
            var priorityBreakdown = await _repo.GetPriorityBreakdownAsync(null, windowStart);
            var recentActivity    = await _repo.GetRecentActivityAsync(take: 10);
            var recentTickets     = await _repo.GetRecentTicketsAsync(take: 10);
            var agentWorkload     = await _repo.GetAgentWorkloadsAsync();

            return new AdminDashboardDto
            {
                OpenTickets       = openTickets,
                PendingTickets    = pendingTickets,
                ResolvedToday     = resolvedToday,
                CriticalTickets   = criticalTickets,
                TotalUsers        = totalUsers,
                ActiveUsers       = activeUsers,
                TotalTickets      = totalTickets,
                EscalatedTickets  = escalatedTickets,
                TicketTrend       = ticketTrend,
                CategoryBreakdown = categoryBreakdown,
                PriorityBreakdown = priorityBreakdown,
                RecentActivity    = recentActivity,
                RecentTickets     = recentTickets,
                AgentWorkload     = agentWorkload
            };
        }

        public async Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId, int days = 30)
        {
            var deptId = await _repo.GetUserDepartmentIdAsync(managerId);
            if (!deptId.HasValue) return new ManagerDashboardDto();

            var windowStart = DateTime.UtcNow.AddDays(-days);

            var teamOpenTickets   = await _repo.CountDeptTicketsByStatusNamesAsync(
                deptId.Value, ["Open", "In Progress"], windowStart);
            var unassignedTickets = await _repo.CountDeptUnassignedTicketsAsync(deptId.Value);
            var resolvedThisWindow = await _repo.CountDeptTicketsResolvedSinceAsync(deptId.Value, windowStart);

            var resolvedTimes = await _repo.GetResolvedTicketTimesAsync(deptId.Value, windowStart);
            var avgResolutionHours = resolvedTimes.Count > 0
                ? Math.Round(resolvedTimes.Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours), 2)
                : 0;

            var escalated = await _repo.CountDeptEscalatedTicketsAsync(deptId.Value);

            var agentRoleId = await _repo.GetAgentRoleIdAsync();
            var agents      = await _repo.GetAgentsInDepartmentAsync(deptId.Value, agentRoleId);
            var agentIds    = agents.Select(a => a.Id).ToList();
            var agentData   = await _repo.GetAgentTicketDataAsync(agentIds);

            var agentPerformance = agents.Select(agent => new AgentPerformanceDto
            {
                AgentName        = agent.FirstName + " " + agent.LastName,
                ResolvedTickets  = agentData.Count(t => t.AgentId == agent.Id && t.StatusName == "Resolved"),
                OpenTickets      = agentData.Count(t => t.AgentId == agent.Id && (t.StatusName == "Open" || t.StatusName == "In Progress")),
                EscalatedTickets = agentData.Count(t => t.AgentId == agent.Id && t.IsEscalated)
            }).ToList();

            var agentAvailability = agents.Select(agent =>
            {
                var openCount = agentData.Count(t =>
                    t.AgentId == agent.Id && (t.StatusName == "Open" || t.StatusName == "In Progress"));
                var resolvedCount = agentData.Count(t =>
                    t.AgentId == agent.Id &&
                    t.StatusName == "Resolved" &&
                    t.UpdatedAt.HasValue && t.UpdatedAt.Value >= windowStart);
                return new AgentAvailabilityDto
                {
                    UserId           = agent.Id,
                    AgentName        = agent.FirstName + " " + agent.LastName,
                    OpenTickets      = openCount,
                    ResolvedThisWeek = resolvedCount,
                    IsAvailable      = openCount < 5
                };
            }).ToList();

            var unassignedList    = await _repo.GetUnassignedTicketsInDeptAsync(deptId.Value, 20);
            var escalatedList     = await _repo.GetEscalatedTicketsNeedingReassignmentAsync(deptId.Value, 20);
            var recentTickets     = await _repo.GetRecentTicketsAsync(take: 10, deptId: deptId.Value);
            var categoryBreakdown = await _repo.GetCategoryBreakdownAsync(deptId.Value, windowStart);
            var priorityBreakdown = await _repo.GetPriorityBreakdownAsync(deptId.Value, windowStart);

            return new ManagerDashboardDto
            {
                TeamOpenTickets       = teamOpenTickets,
                UnassignedTickets     = unassignedTickets,
                ResolvedThisWeek      = resolvedThisWindow,
                AvgResolutionHours    = avgResolutionHours,
                EscalatedTickets      = escalated,
                AgentPerformance      = agentPerformance,
                AgentAvailability     = agentAvailability,
                UnassignedTicketsList = unassignedList,
                EscalatedTicketsList  = escalatedList,
                RecentTickets         = recentTickets,
                CategoryBreakdown     = categoryBreakdown,
                PriorityBreakdown     = priorityBreakdown
            };
        }

        public async Task<AgentDashboardDto> GetAgentDashboardAsync(int agentUserId, int days = 30)
        {
            var today       = DateTime.UtcNow.Date;
            var windowStart = DateTime.UtcNow.AddDays(-days);
            var prevWindowStart = DateTime.UtcNow.AddDays(-days * 2);

            var assignedToMe         = await _repo.CountAgentActiveTicketsAsync(agentUserId);
            var resolvedToday        = await _repo.CountAgentTicketsResolvedTodayAsync(agentUserId);
            var inProgress           = await _repo.CountAgentTicketsByStatusNameAsync(agentUserId, "In Progress", windowStart);
            var resolvedThisWindow   = await _repo.CountAgentTicketsResolvedSinceAsync(agentUserId, windowStart);
            var resolvedPrevWindow   = await _repo.CountAgentTicketsResolvedSinceAsync(agentUserId, prevWindowStart) - resolvedThisWindow;
            var escalatedCount       = await _repo.CountAgentEscalatedTicketsAsync(agentUserId);
            var myTickets            = await _repo.GetRecentTicketsAsync(assignedToUserId: agentUserId);
            var recentActivity       = await _repo.GetRecentActivityAsync(userId: agentUserId, take: 10);

            var resolvedTimes        = await _repo.GetAgentResolvedTicketTimesAsync(agentUserId, windowStart);
            var avgResolutionHours   = resolvedTimes.Count > 0
                ? Math.Round(resolvedTimes.Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours), 2)
                : 0;

            return new AgentDashboardDto
            {
                AssignedToMe          = assignedToMe,
                ResolvedToday         = resolvedToday,
                InProgress            = inProgress,
                ResolvedThisWeek      = resolvedThisWindow,
                EscalatedCount        = escalatedCount,
                AvgResolutionHours    = avgResolutionHours,
                ResolvedPreviousWindow = resolvedPrevWindow < 0 ? 0 : resolvedPrevWindow,
                MyTickets             = myTickets,
                RecentActivity        = recentActivity
            };
        }

        public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeUserId, int days = 30)
        {
            var windowStart = DateTime.UtcNow.AddDays(-days);

            var myOpenTickets       = await _repo.CountEmployeeTicketsByStatusNameAsync(employeeUserId, "Open", windowStart);
            var myInProgressTickets = await _repo.CountEmployeeTicketsByStatusNameAsync(employeeUserId, "In Progress", windowStart);
            var myResolvedTickets   = await _repo.CountEmployeeTicketsByStatusNameAsync(employeeUserId, "Resolved", windowStart);
            var myTotalTickets      = await _repo.CountAllEmployeeTicketsAsync(employeeUserId);
            var myRecentTickets     = await _repo.GetRecentTicketsAsync(take: 10, createdByUserId: employeeUserId);
            var myNotifications     = await _repo.GetUserNotificationsAsync(employeeUserId);

            return new EmployeeDashboardDto
            {
                MyOpenTickets       = myOpenTickets,
                MyInProgressTickets = myInProgressTickets,
                MyResolvedTickets   = myResolvedTickets,
                MyTotalTickets      = myTotalTickets,
                MyRecentTickets     = myRecentTickets,
                MyNotifications     = myNotifications
            };
        }
    }
}
