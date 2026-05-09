using AssetManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.API;

[ApiController]
[Route("api/buildings")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    /// <summary>GET /api/buildings — list all buildings</summary>
    [HttpGet]
    public async Task<IActionResult> GetBuildings()
    {
        var buildings = await _buildingService.GetDropdownAsync();

        var data = buildings.Select(b => new
        {
            id          = b.Id,
            name        = b.Name,
            address     = b.Address,
            floorsCount = b.FloorsCount
        });

        return Ok(new { success = true, data });
    }

    /// <summary>GET /api/buildings/{id}/floors — list floors for a building</summary>
    [HttpGet("{id:guid}/floors")]
    public async Task<IActionResult> GetFloors(Guid id)
    {
        var floors = await _buildingService.GetFloorsForBuildingAsync(id);

        var data = floors.Select(f => new
        {
            id         = f.Id,
            buildingId = f.BuildingId,
            level      = f.Level,
            name       = f.Name
        });

        return Ok(new { success = true, data });
    }
}
