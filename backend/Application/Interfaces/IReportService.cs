using backend.Application.DTOs.Reports;

namespace backend.Application.Interfaces
{
    public interface IReportService
    {
        Task<TicketVolumeReportDto> GetTicketVolumeAsync(DateTime from, DateTime to, int? departmentId = null);
        Task<ResolutionTimeReportDto> GetResolutionTimeAsync(DateTime from, DateTime to, int? departmentId = null);
        Task<List<EmployeeReportRowDto>> GetByEmployeeAsync(DateTime from, DateTime to, int? departmentId = null);
        Task<SummaryReportDto> GetSummaryAsync(DateTime from, DateTime to, int? departmentId = null);
    }
}
