using AssetManagement.Core.Interfaces;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Assets")]
public class AssetsController : Controller
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    // ── Index ────────────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] Guid?   buildingId,
        [FromQuery] string? status      = null,
        [FromQuery] string? criticality = null,
        [FromQuery] string? search      = null,
        [FromQuery] int     page        = 1)
    {
        const int pageSize = 10;
        if (page < 1) page = 1;

        var formVm     = await _assetService.BuildFormViewModelAsync(null);
        var buildings  = formVm.Buildings;
        var selectedId = buildingId ?? (buildings.Count > 0 ? buildings[0].Id : (Guid?)null);

        var paged = await _assetService.GetAssetsPagedAsync(selectedId, page, pageSize, status, criticality, search);

        int operational = 0, underMaint = 0, svcOverdue = 0;
        if (selectedId.HasValue)
            (operational, underMaint, svcOverdue) = await _assetService.GetBuildingStatsAsync(selectedId.Value);

        var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)paged.Total / pageSize) : 1;

        ViewBag.Assets             = paged.Items;
        ViewBag.SelectedBuildingId = selectedId;
        ViewBag.Buildings          = buildings;
        ViewBag.Operational        = operational;
        ViewBag.UnderMaintenance   = underMaint;
        ViewBag.ServiceOverdue     = svcOverdue;
        ViewBag.Total              = paged.Total;
        ViewBag.Page               = page;
        ViewBag.TotalPages         = totalPages;
        ViewBag.HasPrev            = page > 1;
        ViewBag.HasNext            = page < totalPages;
        ViewBag.Status             = status;
        ViewBag.Criticality        = criticality;
        ViewBag.Search             = search;
        return View();
    }

    // ── Details ──────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var asset = await _assetService.GetEntityByIdWithNavAsync(id);
        if (asset == null) return NotFound();
        return View(asset);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        return View(await _assetService.BuildFormViewModelAsync(null));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetFormViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.QRCode))
        {
            var isUnique = await _assetService.IsQRCodeUniqueAsync(model.QRCode, null);
            if (!isUnique)
                ModelState.AddModelError("QRCode", "This QR code is already used by another asset.");
        }

        if (!ModelState.IsValid)
        {
            var refreshed = await _assetService.BuildFormViewModelForBuildingAsync(model.BuildingId);
            model.Buildings = refreshed.Buildings;
            model.Floors    = refreshed.Floors;
            return View(model);
        }

        var asset = await _assetService.CreateAsync(model);

        TempData["Success"] = $"Asset '{asset.Name}' created.";
        return RedirectToAction(nameof(Details), new { id = asset.Id });
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var asset = await _assetService.GetEntityByIdAsync(id);
        if (asset == null) return NotFound();
        return View(await _assetService.BuildFormViewModelAsync(asset));
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AssetFormViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.QRCode))
        {
            var isUnique = await _assetService.IsQRCodeUniqueAsync(model.QRCode, id);
            if (!isUnique)
                ModelState.AddModelError("QRCode", "This QR code is already used by another asset.");
        }

        if (!ModelState.IsValid)
        {
            var refreshed = await _assetService.BuildFormViewModelForBuildingAsync(model.BuildingId);
            model.Buildings = refreshed.Buildings;
            model.Floors    = refreshed.Floors;
            return View(model);
        }

        var updated = await _assetService.UpdateAsync(id, model);
        if (!updated) return NotFound();

        TempData["Success"] = $"Asset '{model.Name}' updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, error) = await _assetService.DeleteAsync(id);

        if (!success && error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!success) return NotFound();

        TempData["Success"] = "Asset deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── AJAX: floors for building ─────────────────────────────────────────────

    [HttpGet("FloorsFor/{buildingId:guid}")]
    public async Task<IActionResult> FloorsFor(Guid buildingId)
    {
        var floors = await _assetService.GetFloorsForBuildingDropdownAsync(buildingId);
        return Json(floors.Select(f => new { id = f.Id, label = f.Label }));
    }
}
