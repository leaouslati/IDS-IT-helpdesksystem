using backend.Application.DTOs;
using backend.Domain.Entities;

namespace backend.Application.Interfaces
{
    public record AgentTicketData(int AgentId, string StatusName, bool IsEscalated, DateTime? UpdatedAt);

    public interface IDashboardRepository
    {
        // ── Admin ────────────────────────────────────────────────────────────
        Task<int> CountTicketsByStatusNameAsync(string statusName, DateTime? since = null);
        Task<int> CountTicketsResolvedOnDateAsync(DateTime date);
        Task<int> CountActiveCriticalTicketsAsync();
        Task<int> CountUsersAsync(bool? isActive = null);
        Task<int> CountAllTicketsAsync(DateTime? since = null);
        Task<int> CountEscalatedTicketsAsync(DateTime? since = null);
        Task<List<TicketTrendDto>> GetTicketTrendAsync(DateTime from);
        Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(int? deptId = null, DateTime? since = null);
        Task<List<PriorityBreakdownDto>> GetPriorityBreakdownAsync(int? deptId = null, DateTime? since = null);
        Task<List<RecentActivityDto>> GetRecentActivityAsync(int? userId = null, int take = 10);
        Task<List<RecentTicketDto>> GetRecentTicketsAsync(
            int take = 10, int? deptId = null, int? assignedToUserId = null, int? createdByUserId = null);
        Task<List<AgentWorkloadDto>> GetAgentWorkloadsAsync();

        // ── Manager ──────────────────────────────────────────────────────────
        Task<int?> GetUserDepartmentIdAsync(int userId);
        Task<int> CountDeptTicketsByStatusNamesAsync(int deptId, IEnumerable<string> statusNames, DateTime? since = null);
        Task<int> CountDeptUnassignedTicketsAsync(int deptId);
        Task<int> CountDeptEscalatedTicketsAsync(int deptId);
        Task<int> CountDeptTicketsResolvedSinceAsync(int deptId, DateTime since);
        Task<List<(DateTime CreatedAt, DateTime UpdatedAt)>> GetResolvedTicketTimesAsync(int deptId, DateTime? since = null);
        Task<int> GetAgentRoleIdAsync();
        Task<List<User>> GetAgentsInDepartmentAsync(int deptId, int agentRoleId);
        Task<List<AgentTicketData>> GetAgentTicketDataAsync(List<int> agentIds);
        Task<List<RecentTicketDto>> GetUnassignedTicketsInDeptAsync(int deptId, int take = 20);
        Task<List<RecentTicketDto>> GetEscalatedTicketsNeedingReassignmentAsync(int deptId, int take = 20);

        // ── Agent ─────────────────────────────────────────────────────────────
        Task<int> CountAgentActiveTicketsAsync(int agentId);
        Task<int> CountAgentTicketsByStatusNameAsync(int agentId, string statusName, DateTime? since = null);
        Task<int> CountAgentTicketsResolvedTodayAsync(int agentId);
        Task<int> CountAgentTicketsResolvedSinceAsync(int agentId, DateTime since);
        Task<int> CountAgentEscalatedTicketsAsync(int agentId);
        Task<List<(DateTime CreatedAt, DateTime UpdatedAt)>> GetAgentResolvedTicketTimesAsync(int agentId, DateTime since);

        // ── Employee ──────────────────────────────────────────────────────────
        Task<int> CountEmployeeTicketsByStatusNameAsync(int employeeId, string statusName, DateTime? since = null);
        Task<int> CountEmployeeTicketsByStatusNamesAsync(int employeeId, IEnumerable<string> statusNames);
        Task<int> CountAllEmployeeTicketsAsync(int employeeId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId);
    }
}
