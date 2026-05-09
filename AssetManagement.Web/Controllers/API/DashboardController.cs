using AssetManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.API;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>GET /api/dashboard/stats — overall KPI stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] Guid? buildingId)
    {
        var stats = await _dashboardService.GetDashboardStatsAsync(buildingId);
        return Ok(new { success = true, data = stats });
    }

    /// <summary>GET /api/dashboard/buildings/performance — per-building metrics</summary>
    [HttpGet("buildings/performance")]
    public async Task<IActionResult> GetBuildingPerformance()
    {
        var performance = await _dashboardService.GetBuildingPerformanceAsync();
        return Ok(new { success = true, data = performance });
    }
}
