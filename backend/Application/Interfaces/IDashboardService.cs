using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();
        Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId);
        Task<AgentDashboardDto> GetAgentDashboardAsync(int agentUserId);
        Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeUserId);
    }
}
