using backend.Application.DTOs.Reports;
using backend.Application.Interfaces;
using ClosedXML.Excel;

namespace backend.Infrastructure.Services
{
    public class ExcelReportService : IExcelReportService
    {
        public byte[] GenerateExcel(SummaryReportDto report, DateTime from, DateTime to)
        {
            using var workbook = new XLWorkbook();

            BuildSummarySheet(workbook, report, from, to);
            BuildByEmployeeSheet(workbook, report);
            BuildDailyBreakdownSheet(workbook, report);

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private static void BuildSummarySheet(XLWorkbook wb, SummaryReportDto report, DateTime from, DateTime to)
        {
            var ws  = wb.Worksheets.Add("Summary");
            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "IT Help Desk Report";
            ws.Cell(row, 1).Style.Font.Bold     = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            row++;
            ws.Cell(row, 1).Value = $"Period: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}";
            row++;
            ws.Cell(row, 1).Value = $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
            row += 2;

            // Ticket Volume
            WriteSection(ws, ref row, "Ticket Volume");
            WriteTwoColHeader(ws, row++);
            WriteKV(ws, row++, "Total Opened", report.Volume.TotalOpened);
            WriteKV(ws, row++, "Total Closed", report.Volume.TotalClosed);
            WriteKV(ws, row++, "Net Change",   report.Volume.NetChange);
            row++;

            // Resolution Time
            WriteSection(ws, ref row, "Resolution Time");
            WriteTwoColHeader(ws, row++);
            WriteKV(ws, row++, "Total Resolved",               report.ResolutionTime.TotalResolved);
            WriteKV(ws, row++, "Avg Resolution Time (hours)",  report.ResolutionTime.AvgHoursOverall);
            row++;

            // By Category
            WriteSection(ws, ref row, "Resolution by Category");
            WriteThreeColHeader(ws, row++, "Category", "Avg Hours", "Count");
            foreach (var c in report.ResolutionTime.ByCategory)
            {
                ws.Cell(row, 1).Value = c.Name;
                ws.Cell(row, 2).Value = c.AvgHours;
                ws.Cell(row, 3).Value = c.Count;
                row++;
            }
            row++;

            // By Priority
            WriteSection(ws, ref row, "Resolution by Priority");
            WriteThreeColHeader(ws, row++, "Priority", "Avg Hours", "Count");
            foreach (var p in report.ResolutionTime.ByPriority)
            {
                ws.Cell(row, 1).Value = p.Name;
                ws.Cell(row, 2).Value = p.AvgHours;
                ws.Cell(row, 3).Value = p.Count;
                row++;
            }

            ws.Columns().AdjustToContents();
        }

        private static void BuildByEmployeeSheet(XLWorkbook wb, SummaryReportDto report)
        {
            var ws      = wb.Worksheets.Add("By Employee");
            var headers = new[] { "Name", "Role", "Total Tickets", "Open", "In Progress", "Pending", "Resolved", "Closed" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold            = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            ws.SheetView.FreezeRows(1);

            int row = 2;
            foreach (var e in report.ByEmployee)
            {
                ws.Cell(row, 1).Value = e.Name;
                ws.Cell(row, 2).Value = e.Role;
                ws.Cell(row, 3).Value = e.TotalTickets;
                ws.Cell(row, 4).Value = e.Open;
                ws.Cell(row, 5).Value = e.InProgress;
                ws.Cell(row, 6).Value = e.Pending;
                ws.Cell(row, 7).Value = e.Resolved;
                ws.Cell(row, 8).Value = e.Closed;
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void BuildDailyBreakdownSheet(XLWorkbook wb, SummaryReportDto report)
        {
            var ws      = wb.Worksheets.Add("Daily Breakdown");
            var headers = new[] { "Date", "Opened", "Closed" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold            = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            ws.SheetView.FreezeRows(1);

            int row = 2;
            foreach (var d in report.Volume.Daily)
            {
                ws.Cell(row, 1).Value = d.Date;
                ws.Cell(row, 2).Value = d.Opened;
                ws.Cell(row, 3).Value = d.Closed;
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void WriteSection(IXLWorksheet ws, ref int row, string title)
        {
            ws.Cell(row, 1).Value           = title;
            ws.Cell(row, 1).Style.Font.Bold     = true;
            ws.Cell(row, 1).Style.Font.FontSize = 12;
            row++;
        }

        private static void WriteTwoColHeader(IXLWorksheet ws, int row)
        {
            foreach (var (col, text) in new[] { (1, "Metric"), (2, "Value") })
            {
                ws.Cell(row, col).Value             = text;
                ws.Cell(row, col).Style.Font.Bold            = true;
                ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
        }

        private static void WriteThreeColHeader(IXLWorksheet ws, int row, string c1, string c2, string c3)
        {
            foreach (var (col, text) in new[] { (1, c1), (2, c2), (3, c3) })
            {
                ws.Cell(row, col).Value             = text;
                ws.Cell(row, col).Style.Font.Bold            = true;
                ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
        }

        private static void WriteKV(IXLWorksheet ws, int row, string key, object value)
        {
            ws.Cell(row, 1).Value = key;
            ws.Cell(row, 2).Value = value?.ToString() ?? "";
        }
    }
}
