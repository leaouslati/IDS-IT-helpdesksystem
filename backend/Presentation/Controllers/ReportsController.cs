using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace backend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService         _reportService;
        private readonly IPdfReportService      _pdfService;
        private readonly IExcelReportService    _excelService;
        private readonly IDashboardRepository   _dashboardRepo;

        public ReportsController(
            IReportService reportService,
            IPdfReportService pdfService,
            IExcelReportService excelService,
            IDashboardRepository dashboardRepo)
        {
            _reportService  = reportService;
            _pdfService     = pdfService;
            _excelService   = excelService;
            _dashboardRepo  = dashboardRepo;
        }

        // Parses "yyyy-MM-dd" query params; defaults to last 30 days.
        private (DateTime from, DateTime to) ParseRange(string? from, string? to)
        {
            var today  = DateTime.UtcNow.Date;
            var toDate = string.IsNullOrEmpty(to)
                ? today
                : DateTime.SpecifyKind(DateTime.ParseExact(to,   "yyyy-MM-dd", CultureInfo.InvariantCulture), DateTimeKind.Utc);
            var fromDate = string.IsNullOrEmpty(from)
                ? today.AddDays(-29)
                : DateTime.SpecifyKind(DateTime.ParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture), DateTimeKind.Utc);
            return (fromDate, toDate);
        }

        // Returns the Manager's department ID so all queries are scoped to their dept.
        // Returns null for Admin (sees all data).
        private async Task<int?> ManagerDeptIdAsync()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Manager") return null;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _dashboardRepo.GetUserDepartmentIdAsync(userId);
        }

        [HttpGet("ticket-volume")]
        public async Task<IActionResult> GetTicketVolume([FromQuery] string? from, [FromQuery] string? to)
        {
            var (f, t)  = ParseRange(from, to);
            var deptId  = await ManagerDeptIdAsync();
            return Ok(await _reportService.GetTicketVolumeAsync(f, t, deptId));
        }

        [HttpGet("resolution-time")]
        public async Task<IActionResult> GetResolutionTime([FromQuery] string? from, [FromQuery] string? to)
        {
            var (f, t)  = ParseRange(from, to);
            var deptId  = await ManagerDeptIdAsync();
            return Ok(await _reportService.GetResolutionTimeAsync(f, t, deptId));
        }

        [HttpGet("by-employee")]
        public async Task<IActionResult> GetByEmployee([FromQuery] string? from, [FromQuery] string? to)
        {
            var (f, t)  = ParseRange(from, to);
            var deptId  = await ManagerDeptIdAsync();
            return Ok(await _reportService.GetByEmployeeAsync(f, t, deptId));
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string? from, [FromQuery] string? to)
        {
            var (f, t)  = ParseRange(from, to);
            var deptId  = await ManagerDeptIdAsync();
            return Ok(await _reportService.GetSummaryAsync(f, t, deptId));
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] string? from, [FromQuery] string? to)
        {
            var (f, t)  = ParseRange(from, to);
            var deptId  = await ManagerDeptIdAsync();
            var summary = await _reportService.GetSummaryAsync(f, t, deptId);
            var bytes   = _pdfService.GeneratePdf(summary, f, t);
            return File(bytes, "application/pdf", $"HelpDesk_Report_{f:yyyy-MM-dd}_{t:yyyy-MM-dd}.pdf");
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? from, [FromQuery] string? to)
        {
            var (f, t)  = ParseRange(from, to);
            var deptId  = await ManagerDeptIdAsync();
            var summary = await _reportService.GetSummaryAsync(f, t, deptId);
            var bytes   = _excelService.GenerateExcel(summary, f, t);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HelpDesk_Report_{f:yyyy-MM-dd}_{t:yyyy-MM-dd}.xlsx");
        }
    }
}
