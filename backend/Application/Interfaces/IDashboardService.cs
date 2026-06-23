using backend.Application.DTOs;

namespace backend.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto>    GetAdminDashboardAsync(int days = 30);
        Task<ManagerDashboardDto>  GetManagerDashboardAsync(int managerId, int days = 30);
        Task<AgentDashboardDto>    GetAgentDashboardAsync(int agentUserId, int days = 30);
        Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeUserId, int days = 30);
    }
}
