using backend.Application.DTOs.Reports;

namespace backend.Application.Interfaces
{
    public interface IExcelReportService
    {
        byte[] GenerateExcel(SummaryReportDto report, DateTime from, DateTime to);
    }
}
