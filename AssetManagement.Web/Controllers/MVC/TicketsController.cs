using System.Security.Claims;
using AssetManagement.Core.Interfaces;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Tickets")]
public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly IVendorService _vendorService;
    private readonly IUserService   _userService;
    private readonly IAssetService  _assetService;
    private readonly IBuildingService _buildingService;

    public TicketsController(
        ITicketService    ticketService,
        IVendorService    vendorService,
        IUserService      userService,
        IAssetService     assetService,
        IBuildingService  buildingService)
    {
        _ticketService   = ticketService;
        _vendorService   = vendorService;
        _userService     = userService;
        _assetService    = assetService;
        _buildingService = buildingService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] TicketFilterDto filter)
    {
        filter.PageSize = 20;
        if (filter.Page < 1) filter.Page = 1;
        var result = await _ticketService.GetTicketsAsync(filter);

        ViewBag.Buildings = await _buildingService.GetDropdownAsync();

        var vm = new TicketListViewModel
        {
            Tickets  = result.Items,
            Total    = result.Total,
            Page     = result.Page,
            PageSize = result.PageSize,
            Filter   = filter
        };

        return View(vm);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound();

        var vendors         = (await _vendorService.GetAllAsync())
                              .Select(v => new VendorDropdownDto { Id = v.Id, Name = v.Name })
                              .ToList();
        var facilitiesUsers = await _userService.GetFacilitiesUsersAsync();

        var vm = new TicketDetailsViewModel
        {
            Ticket          = ticket,
            Vendors         = vendors,
            FacilitiesUsers = facilitiesUsers
        };

        return View(vm);
    }

    [HttpPost("UpdateStatus/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTicketStatusDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        try
        {
            await _ticketService.UpdateStatusAsync(id, dto, userId);
            TempData["Success"] = "Ticket status updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost("Close/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id, CloseTicketDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        try
        {
            await _ticketService.CloseTicketAsync(id, dto, userId);
            TempData["Success"] = "Ticket closed successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var assets = await _assetService.GetAssetDropdownAsync();
        return View(new CreateTicketViewModel { Assets = assets });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Assets = await _assetService.GetAssetDropdownAsync();
            return View(model);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        try
        {
            var ticket = await _ticketService.CreateTicketAsync(new CreateTicketDto
            {
                AssetId     = model.AssetId,
                Title       = model.Title,
                Description = model.Description,
                Priority    = model.Priority
            }, userId);

            TempData["Success"] = $"Ticket {ticket.TicketNumber} created successfully.";
            return RedirectToAction("Details", new { id = ticket.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
