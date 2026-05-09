using AssetManagement.Core.Interfaces;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Dashboard")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ITicketService    _ticketService;
    private readonly IBuildingService  _buildingService;

    public DashboardController(
        IDashboardService dashboardService,
        ITicketService    ticketService,
        IBuildingService  buildingService)
    {
        _dashboardService = dashboardService;
        _ticketService    = ticketService;
        _buildingService  = buildingService;
    }

    [HttpGet("")]
    [HttpGet("/")]
    public async Task<IActionResult> Index([FromQuery] Guid? buildingId)
    {
        var stats        = await _dashboardService.GetDashboardStatsAsync(buildingId);
        var buildingPerf = await _dashboardService.GetBuildingPerformanceAsync();
        var buildings    = await _buildingService.GetDropdownAsync();

        var recentTickets = await _ticketService.GetTicketsAsync(new TicketFilterDto
        {
            BuildingId = buildingId,
            Page       = 1,
            PageSize   = 8
        });

        var vm = new DashboardViewModel
        {
            Stats               = stats,
            BuildingPerformance = buildingPerf.ToList(),
            RecentTickets       = recentTickets.Items,
            Buildings           = buildings,
            SelectedBuildingId  = buildingId
        };

        return View(vm);
    }
}
