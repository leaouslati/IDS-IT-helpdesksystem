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

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var today      = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-6);

            var openTickets      = await _repo.CountTicketsByStatusNameAsync("Open");
            var pendingTickets   = await _repo.CountTicketsByStatusNameAsync("Pending");
            var resolvedToday    = await _repo.CountTicketsResolvedOnDateAsync(today);
            var criticalTickets  = await _repo.CountActiveCriticalTicketsAsync();
            var totalUsers       = await _repo.CountUsersAsync();
            var activeUsers      = await _repo.CountUsersAsync(true);
            var totalTickets     = await _repo.CountAllTicketsAsync();
            var escalatedTickets = await _repo.CountEscalatedTicketsAsync();
            var ticketTrend      = await _repo.GetTicketTrendAsync(sevenDaysAgo);
            var categoryBreakdown = await _repo.GetCategoryBreakdownAsync();
            var priorityBreakdown = await _repo.GetPriorityBreakdownAsync();
            var recentActivity   = await _repo.GetRecentActivityAsync(take: 10);
            var recentTickets    = await _repo.GetRecentTicketsAsync(take: 10);

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
                RecentTickets     = recentTickets
            };
        }

        public async Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId)
        {
            var deptId = await _repo.GetUserDepartmentIdAsync(managerId);
            if (!deptId.HasValue) return new ManagerDashboardDto();

            var weekStart = DateTime.UtcNow.Date.AddDays(-6);

            var teamOpenTickets   = await _repo.CountDeptTicketsByStatusNamesAsync(deptId.Value, ["Open", "In Progress"]);
            var unassignedTickets = await _repo.CountDeptUnassignedTicketsAsync(deptId.Value);
            var resolvedThisWeek  = await _repo.CountDeptTicketsResolvedSinceAsync(deptId.Value, weekStart);

            var resolvedTimes = await _repo.GetResolvedTicketTimesAsync(deptId.Value);
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
                AgentName       = agent.FirstName + " " + agent.LastName,
                ResolvedTickets = agentData.Count(t => t.AgentId == agent.Id && t.StatusName == "Resolved"),
                OpenTickets     = agentData.Count(t => t.AgentId == agent.Id && (t.StatusName == "Open" || t.StatusName == "In Progress")),
                EscalatedTickets = agentData.Count(t => t.AgentId == agent.Id && t.IsEscalated)
            }).ToList();

            var agentAvailability = agents.Select(agent =>
            {
                var openCount = agentData.Count(t =>
                    t.AgentId == agent.Id && (t.StatusName == "Open" || t.StatusName == "In Progress"));
                var resolvedCount = agentData.Count(t =>
                    t.AgentId == agent.Id &&
                    t.StatusName == "Resolved" &&
                    t.UpdatedAt.HasValue && t.UpdatedAt.Value >= weekStart);
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
            var categoryBreakdown = await _repo.GetCategoryBreakdownAsync(deptId.Value);
            var priorityBreakdown = await _repo.GetPriorityBreakdownAsync(deptId.Value);

            return new ManagerDashboardDto
            {
                TeamOpenTickets       = teamOpenTickets,
                UnassignedTickets     = unassignedTickets,
                ResolvedThisWeek      = resolvedThisWeek,
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

        public async Task<AgentDashboardDto> GetAgentDashboardAsync(int agentUserId)
        {
            var today     = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);

            var assignedToMe     = await _repo.CountAgentActiveTicketsAsync(agentUserId);
            var resolvedToday    = await _repo.CountAgentTicketsResolvedTodayAsync(agentUserId);
            var inProgress       = await _repo.CountAgentTicketsByStatusNameAsync(agentUserId, "In Progress");
            var resolvedThisWeek = await _repo.CountAgentTicketsResolvedSinceAsync(agentUserId, weekStart);
            var escalatedCount   = await _repo.CountAgentEscalatedTicketsAsync(agentUserId);
            var myTickets        = await _repo.GetRecentTicketsAsync(assignedToUserId: agentUserId);
            var recentActivity   = await _repo.GetRecentActivityAsync(userId: agentUserId, take: 10);

            return new AgentDashboardDto
            {
                AssignedToMe     = assignedToMe,
                ResolvedToday    = resolvedToday,
                InProgress       = inProgress,
                ResolvedThisWeek = resolvedThisWeek,
                EscalatedCount   = escalatedCount,
                MyTickets        = myTickets,
                RecentActivity   = recentActivity
            };
        }

        public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeUserId)
        {
            var myOpenTickets       = await _repo.CountEmployeeTicketsByStatusNameAsync(employeeUserId, "Open");
            var myInProgressTickets = await _repo.CountEmployeeTicketsByStatusNameAsync(employeeUserId, "In Progress");
            var myResolvedTickets   = await _repo.CountEmployeeTicketsByStatusNameAsync(employeeUserId, "Resolved");
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
