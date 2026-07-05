using backend.Application.DTOs.Reports;
using backend.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace backend.Infrastructure.Services
{
    public class PdfReportService : IPdfReportService
    {
        public byte[] GeneratePdf(SummaryReportDto report, DateTime from, DateTime to)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    // ── Header ──────────────────────────────────────────────────────
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("IT Help Desk — Report").Bold().FontSize(18).FontColor("#0F172A");
                                c.Item().Text($"Period: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}").FontSize(10).FontColor("#6B7280");
                                c.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(9).FontColor("#9CA3AF");
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#14B8A6");
                    });

                    // ── Content ─────────────────────────────────────────────────────
                    page.Content().PaddingTop(18).Column(col =>
                    {
                        // ─── Section 1: Ticket Volume ────────────────────────────────
                        col.Item().Text("1. Ticket Volume").Bold().FontSize(14).FontColor("#0F172A");
                        col.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text($"Total Opened:  {report.Volume.TotalOpened}").FontSize(10);
                            row.RelativeItem().Text($"Total Closed:  {report.Volume.TotalClosed}").FontSize(10);
                            row.RelativeItem().Text($"Net Change:    {report.Volume.NetChange:+#;-#;0}").FontSize(10);
                        });

                        // Visual bar chart: each day is a row with colored proportional bars
                        if (report.Volume.Daily.Count > 0)
                        {
                            var dailyViz = report.Volume.Daily.Take(30).ToList();
                            float maxV = dailyViz.Max(d => Math.Max(d.Opened, d.Closed));

                            if (maxV > 0)
                            {
                                col.Item().PaddingTop(8).Text("Daily trend  (blue = opened | teal = closed)").FontSize(9).FontColor("#6B7280");
                                col.Item().PaddingTop(4).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.ConstantColumn(48); // Date label
                                        cols.RelativeColumn(3);  // Opened bar
                                        cols.ConstantColumn(22); // Opened count
                                        cols.RelativeColumn(3);  // Closed bar
                                        cols.ConstantColumn(22); // Closed count
                                    });

                                    table.Header(h =>
                                    {
                                        h.Cell().Padding(2).Text("Date").Bold().FontSize(8);
                                        h.Cell().ColumnSpan(2).Padding(2).Text("Opened").Bold().FontSize(8).FontColor("#3B82F6");
                                        h.Cell().ColumnSpan(2).Padding(2).Text("Closed").Bold().FontSize(8).FontColor("#14B8A6");
                                    });

                                    foreach (var d in dailyViz)
                                    {
                                        float oR = Math.Clamp(d.Opened / maxV, 0.001f, 0.999f);
                                        float cR = Math.Clamp(d.Closed  / maxV, 0.001f, 0.999f);

                                        table.Cell().Padding(2).Text(d.Date).FontSize(7);

                                        // Opened bar
                                        table.Cell().PaddingVertical(2).Row(r =>
                                        {
                                            r.RelativeItem(oR).Background("#3B82F6").Height(8);
                                            r.RelativeItem(1f - oR).Height(8);
                                        });
                                        table.Cell().Padding(2).Text(d.Opened.ToString()).FontSize(7);

                                        // Closed bar
                                        table.Cell().PaddingVertical(2).Row(r =>
                                        {
                                            r.RelativeItem(cR).Background("#14B8A6").Height(8);
                                            r.RelativeItem(1f - cR).Height(8);
                                        });
                                        table.Cell().Padding(2).Text(d.Closed.ToString()).FontSize(7);
                                    }
                                });
                            }
                        }

                        // Daily numbers table (full range, for the "underlying numbers" requirement)
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                            });
                            table.Header(h =>
                            {
                                h.Cell().Background("#F1F5F9").Padding(4).Text("Date").Bold().FontSize(9);
                                h.Cell().Background("#F1F5F9").Padding(4).Text("Opened").Bold().FontSize(9);
                                h.Cell().Background("#F1F5F9").Padding(4).Text("Closed").Bold().FontSize(9);
                            });
                            foreach (var d in report.Volume.Daily.Take(60))
                            {
                                table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(d.Date).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(d.Opened.ToString()).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(d.Closed.ToString()).FontSize(9);
                            }
                        });

                        // ─── Section 2: Resolution Time ──────────────────────────────
                        col.Item().PaddingTop(20).Text("2. Resolution Time").Bold().FontSize(14).FontColor("#0F172A");
                        col.Item().PaddingTop(4).Text(
                            $"System-wide average: {report.ResolutionTime.AvgHoursOverall:F1} hours " +
                            $"({report.ResolutionTime.TotalResolved} ticket{(report.ResolutionTime.TotalResolved != 1 ? "s" : "")} resolved)"
                        ).FontSize(10);

                        // By Category
                        col.Item().PaddingTop(10).Text("By Category").Bold().FontSize(11);
                        if (report.ResolutionTime.ByCategory.Count > 0)
                        {
                            col.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(4);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(2);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Background("#F1F5F9").Padding(4).Text("Category").Bold().FontSize(9);
                                    h.Cell().Background("#F1F5F9").Padding(4).Text("Avg Hours").Bold().FontSize(9);
                                    h.Cell().Background("#F1F5F9").Padding(4).Text("Count").Bold().FontSize(9);
                                });
                                foreach (var r in report.ResolutionTime.ByCategory)
                                {
                                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(r.Name).FontSize(9);
                                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text($"{r.AvgHours:F1}").FontSize(9);
                                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(r.Count.ToString()).FontSize(9);
                                }
                            });
                        }
                        else
                        {
                            col.Item().PaddingTop(4).Text("No resolved tickets in this period.").FontSize(9).FontColor("#9CA3AF");
                        }

                        // By Priority
                        col.Item().PaddingTop(12).Text("By Priority").Bold().FontSize(11);
                        if (report.ResolutionTime.ByPriority.Count > 0)
                        {
                            col.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(4);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(2);
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Background("#F1F5F9").Padding(4).Text("Priority").Bold().FontSize(9);
                                    h.Cell().Background("#F1F5F9").Padding(4).Text("Avg Hours").Bold().FontSize(9);
                                    h.Cell().Background("#F1F5F9").Padding(4).Text("Count").Bold().FontSize(9);
                                });
                                foreach (var r in report.ResolutionTime.ByPriority)
                                {
                                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(r.Name).FontSize(9);
                                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text($"{r.AvgHours:F1}").FontSize(9);
                                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(r.Count.ToString()).FontSize(9);
                                }
                            });
                        }
                        else
                        {
                            col.Item().PaddingTop(4).Text("No resolved tickets in this period.").FontSize(9).FontColor("#9CA3AF");
                        }

                        // ─── Section 3: Employee / Agent Breakdown ───────────────────
                        col.Item().PaddingTop(20).Text("3. Employee / Agent Breakdown").Bold().FontSize(14).FontColor("#0F172A");

                        if (report.ByEmployee.Count > 0)
                        {
                            col.Item().PaddingTop(6).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3); // Name
                                    cols.RelativeColumn(2); // Role
                                    cols.RelativeColumn(1); // Total
                                    cols.RelativeColumn(1); // Open
                                    cols.RelativeColumn(1); // In Prog
                                    cols.RelativeColumn(1); // Pending
                                    cols.RelativeColumn(1); // Resolved
                                    cols.RelativeColumn(1); // Closed
                                });
                                table.Header(h =>
                                {
                                    foreach (var hdr in new[] { "Name", "Role", "Total", "Open", "In Prog", "Pending", "Resolved", "Closed" })
                                        h.Cell().Background("#F1F5F9").Padding(3).Text(hdr).Bold().FontSize(8);
                                });
                                foreach (var emp in report.ByEmployee)
                                {
                                    var vals = new object[] { emp.Name, emp.Role, emp.TotalTickets, emp.Open, emp.InProgress, emp.Pending, emp.Resolved, emp.Closed };
                                    foreach (var v in vals)
                                        table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(3).Text(v?.ToString() ?? "").FontSize(8);
                                }
                            });
                        }
                        else
                        {
                            col.Item().PaddingTop(4).Text("No employee data for this period.").FontSize(9).FontColor("#9CA3AF");
                        }
                    });

                    // ── Footer ──────────────────────────────────────────────────────
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ").FontSize(9).FontColor("#9CA3AF");
                        text.CurrentPageNumber().FontSize(9).FontColor("#9CA3AF");
                        text.Span(" of ").FontSize(9).FontColor("#9CA3AF");
                        text.TotalPages().FontSize(9).FontColor("#9CA3AF");
                    });
                });
            }).GeneratePdf();
        }
    }
}
