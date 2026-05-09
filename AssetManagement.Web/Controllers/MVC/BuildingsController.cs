using AssetManagement.Core.Interfaces;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Buildings")]
public class BuildingsController : Controller
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    // ── Index ────────────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var buildings = await _buildingService.GetAllWithDetailsAsync();
        return View(buildings);
    }

    // ── Details ──────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var building = await _buildingService.GetByIdWithDetailsAsync(id);
        if (building == null) return NotFound();
        return View(building);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [HttpGet("Create")]
    public IActionResult Create() => View(new BuildingFormViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BuildingFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var building = await _buildingService.CreateAsync(model);

        TempData["Success"] = $"Building '{building.Name}' created.";
        return RedirectToAction(nameof(Details), new { id = building.Id });
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var building = await _buildingService.GetByIdAsync(id);
        if (building == null) return NotFound();

        return View(new BuildingFormViewModel
        {
            Id          = building.Id,
            Name        = building.Name,
            Address     = building.Address,
            FloorsCount = building.FloorsCount
        });
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, BuildingFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var updated = await _buildingService.UpdateAsync(id, model);
        if (!updated) return NotFound();

        TempData["Success"] = $"Building '{model.Name}' updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, error) = await _buildingService.DeleteAsync(id);

        if (!success && error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!success) return NotFound();

        TempData["Success"] = "Building deleted.";
        return RedirectToAction(nameof(Index));
    }
}
