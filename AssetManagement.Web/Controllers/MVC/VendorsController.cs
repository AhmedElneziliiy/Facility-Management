using AssetManagement.Core.Interfaces;
using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Vendors")]
public class VendorsController : Controller
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    // ── Index ────────────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var vendors = await _vendorService.GetAllAsync();

        var paged = new PagedList<Vendor>
        {
            Items    = vendors,
            Page     = 1,
            PageSize = vendors.Count,
            Total    = vendors.Count
        };

        return View(paged);
    }

    // ── Details ──────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var vendor = await _vendorService.GetByIdWithTicketsAsync(id);
        if (vendor == null) return NotFound();
        return View(vendor);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [HttpGet("Create")]
    public IActionResult Create() => View(new CreateVendorViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVendorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var vendor = await _vendorService.CreateAsync(model);

        TempData["Success"] = $"Vendor '{vendor.Name}' created successfully.";
        return RedirectToAction(nameof(Details), new { id = vendor.Id });
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var vendor = await _vendorService.GetByIdAsync(id);
        if (vendor == null) return NotFound();

        return View(new EditVendorViewModel
        {
            Id           = vendor.Id,
            Name         = vendor.Name,
            ContactName  = vendor.ContactName,
            ContactPhone = vendor.ContactPhone,
            ContactEmail = vendor.ContactEmail
        });
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditVendorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var updated = await _vendorService.UpdateAsync(id, model);
        if (!updated) return NotFound();

        TempData["Success"] = $"Vendor '{model.Name}' updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, error) = await _vendorService.DeleteAsync(id);

        if (!success && error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!success) return NotFound();

        TempData["Success"] = "Vendor deleted.";
        return RedirectToAction(nameof(Index));
    }
}
