using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Identity;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.MVC;

[Authorize]
[Route("Users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // ── Index ────────────────────────────────────────────────────────────────

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllUsersAsync();

        var paged = new PagedList<ApplicationUser>
        {
            Items    = users,
            Page     = 1,
            PageSize = users.Count,
            Total    = users.Count
        };

        return View(paged);
    }

    // ── Details ──────────────────────────────────────────────────────────────

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.TicketsCreated  = await _userService.GetTicketsCreatedCountAsync(id);
        ViewBag.TicketsAssigned = await _userService.GetTicketsAssignedCountAsync(id);

        return View(user);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [HttpGet("Create")]
    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error, userId) = await _userService.CreateAsync(model);
        if (!success)
        {
            ModelState.AddModelError(
                error == "Username already exists." ? "Username" : string.Empty,
                error ?? "Failed to create user.");
            return View(model);
        }

        TempData["Success"] = $"User '{model.Username}' created successfully.";
        return RedirectToAction(nameof(Details), new { id = userId });
    }

    // ── Edit ─────────────────────────────────────────────────────────────────

    [HttpGet("{id}/Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        return View(new EditUserViewModel
        {
            Id       = user.Id,
            FullName = user.FullName,
            Email    = user.Email ?? string.Empty,
            Role     = user.Role
        });
    }

    [HttpPost("{id}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error) = await _userService.UpdateAsync(id, model);
        if (!success)
        {
            if (error == null) return NotFound();
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        var user = await _userService.GetByIdAsync(id);
        TempData["Success"] = $"User '{user?.UserName}' updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Toggle Active ─────────────────────────────────────────────────────────

    [HttpPost("{id}/ToggleActive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var isNowActive = await _userService.ToggleActiveAsync(id);

        TempData["Success"] = $"User '{user.UserName}' {(isNowActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }

    // ── Reset Password ────────────────────────────────────────────────────────

    [HttpPost("{id}/ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var (success, error) = await _userService.ResetPasswordAsync(id, newPassword);

        TempData[success ? "Success" : "Error"] = success
            ? $"Password for '{user.UserName}' reset successfully."
            : error;

        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [HttpPost("{id}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var (success, error) = await _userService.DeleteAsync(id);

        if (!success && error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = $"User '{user.UserName}' deleted.";
        return RedirectToAction(nameof(Index));
    }
}
