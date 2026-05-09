using AssetManagement.Core.Interfaces;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Floors")]
public class FloorsController : Controller
{
    private readonly IFloorService    _floorService;
    private readonly IBuildingService _buildingService;

    public FloorsController(IFloorService floorService, IBuildingService buildingService)
    {
        _floorService    = floorService;
        _buildingService = buildingService;
    }

    // ── Index (all floors, grouped by building) ──────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var floors = await _floorService.GetAllWithBuildingAsync();
        return View(floors);
    }

    // ── Details ──────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var floor = await _floorService.GetByIdWithDetailsAsync(id);
        if (floor == null) return NotFound();
        return View(floor);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [HttpGet("Create")]
    public async Task<IActionResult> Create([FromQuery] Guid? buildingId)
    {
        var vm = new FloorFormViewModel { BuildingId = buildingId ?? Guid.Empty };

        if (buildingId.HasValue)
        {
            var building = await _buildingService.GetByIdAsync(buildingId.Value);
            vm.BuildingName = building?.Name ?? string.Empty;
        }

        ViewBag.Buildings = await _buildingService.GetDropdownAsync();
        return View(vm);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FloorFormViewModel model)
    {
        var isUnique = await _floorService.IsLevelUniqueAsync(model.BuildingId, model.Level, null);
        if (!isUnique)
            ModelState.AddModelError("Level", $"Floor {model.Level} already exists in this building.");

        if (!ModelState.IsValid)
        {
            ViewBag.Buildings = await _buildingService.GetDropdownAsync();
            return View(model);
        }

        var floor = await _floorService.CreateAsync(model);

        TempData["Success"] = $"Floor {floor.Level} added to building.";
        return RedirectToAction("Details", "Buildings", new { id = floor.BuildingId });
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var floor = await _floorService.GetByIdWithDetailsAsync(id);
        if (floor == null) return NotFound();

        return View(new FloorFormViewModel
        {
            Id           = floor.Id,
            BuildingId   = floor.BuildingId,
            BuildingName = floor.Building.Name,
            Level        = floor.Level,
            Name         = floor.Name
        });
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, FloorFormViewModel model)
    {
        var isUnique = await _floorService.IsLevelUniqueAsync(model.BuildingId, model.Level, id);
        if (!isUnique)
            ModelState.AddModelError("Level", $"Floor {model.Level} already exists in this building.");

        if (!ModelState.IsValid) return View(model);

        var floor = await _floorService.GetByIdAsync(id);
        if (floor == null) return NotFound();

        await _floorService.UpdateAsync(id, model);

        TempData["Success"] = $"Floor {model.Level} updated.";
        return RedirectToAction("Details", "Buildings", new { id = floor.BuildingId });
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var floor = await _floorService.GetByIdAsync(id);
        if (floor == null) return NotFound();

        var buildingId = floor.BuildingId;
        var level      = floor.Level;

        var (success, error) = await _floorService.DeleteAsync(id);

        if (!success && error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Details", "Buildings", new { id = buildingId });
        }

        TempData["Success"] = $"Floor {level} deleted.";
        return RedirectToAction("Details", "Buildings", new { id = buildingId });
    }
}
