using backend.Application.DTOs.Reports;

namespace backend.Application.Interfaces
{
    public interface IPdfReportService
    {
        byte[] GeneratePdf(SummaryReportDto report, DateTime from, DateTime to);
    }
}
