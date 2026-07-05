using backend.Application.DTOs.Reports;

namespace backend.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<List<DailyVolumeDto>> GetDailyVolumeAsync(DateTime from, DateTime to, int? departmentId = null);

        Task<List<(DateTime CreatedAt, DateTime ClosedAt, string Category, string Priority)>> GetResolvedTicketTimesAsync(DateTime from, DateTime to, int? departmentId = null);

        // When departmentId is provided (Manager role), only users in that department are returned.
        Task<List<EmployeeReportRowDto>> GetEmployeeBreakdownAsync(DateTime from, DateTime to, int? departmentId = null);
    }
}
