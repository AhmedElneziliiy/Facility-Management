using AssetManagement.Core.Interfaces;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Reports")]
public class ReportsController : Controller
{
    private readonly ITicketService   _ticketService;
    private readonly IBuildingService _buildingService;

    public ReportsController(ITicketService ticketService, IBuildingService buildingService)
    {
        _ticketService   = ticketService;
        _buildingService = buildingService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ReportFilterViewModel filter)
    {
        filter.PageSize = 20;
        if (filter.Page < 1) filter.Page = 1;
        var dto = new TicketFilterDto
        {
            Status     = filter.Status,
            Priority   = filter.Priority,
            BuildingId = filter.BuildingId,
            Page       = filter.Page > 0 ? filter.Page : 1,
            PageSize   = filter.PageSize
        };

        var result    = await _ticketService.GetTicketsAsync(dto);
        var buildings = await _buildingService.GetDropdownAsync();

        var vm = new ReportsViewModel
        {
            Tickets   = result.Items,
            Total     = result.Total,
            Page      = result.Page,
            PageSize  = result.PageSize,
            Filter    = filter,
            Buildings = buildings
        };

        return View(vm);
    }

    [HttpGet("ExportExcel")]
    public async Task<IActionResult> ExportExcel([FromQuery] ReportFilterViewModel filter)
    {
        var dto = new TicketFilterDto
        {
            Status     = filter.Status,
            Priority   = filter.Priority,
            BuildingId = filter.BuildingId,
            Page       = 1,
            PageSize   = 10000 // export all
        };

        var result = await _ticketService.GetTicketsAsync(dto);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Tickets");

        // Header row
        var headers = new[] { "#", "Ticket No.", "Title", "Asset", "Building", "Floor", "Priority", "Status", "Created By", "Assigned To", "Created At", "Due At", "Closed At", "Overdue" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        int row = 2;
        foreach (var t in result.Items)
        {
            ws.Cell(row, 1).Value  = row - 1;
            ws.Cell(row, 2).Value  = t.TicketNumber;
            ws.Cell(row, 3).Value  = t.Title;
            ws.Cell(row, 4).Value  = t.AssetName;
            ws.Cell(row, 5).Value  = t.BuildingName;
            ws.Cell(row, 6).Value  = t.FloorLevel.HasValue ? $"Floor {t.FloorLevel}" : "—";
            ws.Cell(row, 7).Value  = t.Priority;
            ws.Cell(row, 8).Value  = t.Status;
            ws.Cell(row, 9).Value  = t.CreatedByUserName;
            ws.Cell(row, 10).Value = t.AssignedToUserName ?? "—";
            ws.Cell(row, 11).Value = t.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 12).Value = t.DueAt.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 13).Value = "—";
            ws.Cell(row, 14).Value = t.IsOverdue ? "Yes" : "No";

            if (t.IsOverdue)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#fee2e2");

            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var filename = $"tickets_report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    [HttpGet("ExportPdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] ReportFilterViewModel filter)
    {
        var dto = new TicketFilterDto
        {
            Status     = filter.Status,
            Priority   = filter.Priority,
            BuildingId = filter.BuildingId,
            Page       = 1,
            PageSize   = 10000
        };
        var result    = await _ticketService.GetTicketsAsync(dto);
        var buildings = await _buildingService.GetDropdownAsync();
        var vm = new ReportsViewModel
        {
            Tickets   = result.Items,
            Total     = result.Total,
            Page      = 1,
            PageSize  = result.Total,
            Filter    = filter,
            Buildings = buildings
        };
        return View("PdfReport", vm);
    }

    [HttpGet("ExportCsv")]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterViewModel filter)
    {
        var dto = new TicketFilterDto
        {
            Status     = filter.Status,
            Priority   = filter.Priority,
            BuildingId = filter.BuildingId,
            Page       = 1,
            PageSize   = 10000
        };

        var result = await _ticketService.GetTicketsAsync(dto);

        var lines = new List<string>
        {
            "Ticket No.,Title,Asset,Building,Floor,Priority,Status,Created By,Assigned To,Created At,Due At,Overdue"
        };

        foreach (var t in result.Items)
        {
            lines.Add(string.Join(",",
                t.TicketNumber,
                $"\"{t.Title.Replace("\"", "\"\"")}\"",
                t.AssetName,
                t.BuildingName,
                t.FloorLevel.HasValue ? $"Floor {t.FloorLevel}" : "",
                t.Priority,
                t.Status,
                t.CreatedByUserName,
                t.AssignedToUserName ?? "",
                t.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                t.DueAt.ToString("yyyy-MM-dd HH:mm"),
                t.IsOverdue ? "Yes" : "No"
            ));
        }

        var csv      = string.Join("\n", lines);
        var bytes    = System.Text.Encoding.UTF8.GetBytes(csv);
        var filename = $"tickets_report_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", filename);
    }
}
