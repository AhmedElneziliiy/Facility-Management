using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Identity;
using AssetManagement.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context     = context;
    }

    public async Task<List<ApplicationUser>> GetAllUsersAsync()
    {
        return await _userManager.Users
            .OrderBy(u => u.Role).ThenBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _userManager.FindByNameAsync(username) != null;
    }

    public async Task<(bool Success, string? Error, string? UserId)> CreateAsync(CreateUserViewModel model)
    {
        if (await UsernameExistsAsync(model.Username))
            return (false, "Username already exists.", null);

        var user = new ApplicationUser
        {
            UserName       = model.Username,
            Email          = model.Email,
            FullName       = model.FullName,
            Role           = model.Role,
            IsActive       = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)), null);

        return (true, null, user.Id);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(string id, EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, null);

        user.FullName = model.FullName;
        user.Email    = model.Email;
        user.Role     = model.Role;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    public async Task<bool> ToggleActiveAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return user.IsActive;
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, null);

        var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, null);

        var hasTickets = await _context.Tickets.AnyAsync(t =>
            t.CreatedByUserId == id || t.AssignedToUserId == id);
        if (hasTickets)
            return (false, "Cannot delete — user has linked tickets.");

        await _userManager.DeleteAsync(user);
        return (true, null);
    }

    public async Task<int> GetTicketsCreatedCountAsync(string userId)
    {
        return await _context.Tickets.CountAsync(t => t.CreatedByUserId == userId);
    }

    public async Task<int> GetTicketsAssignedCountAsync(string userId)
    {
        return await _context.Tickets.CountAsync(t => t.AssignedToUserId == userId);
    }

    public async Task<List<UserDropdownDto>> GetFacilitiesUsersAsync()
    {
        return await _userManager.Users
            .Where(u => u.Role == "facilities" && u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new UserDropdownDto { Id = u.Id, FullName = u.FullName })
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetFacilitiesUsersFullAsync()
    {
        return await _userManager.Users
            .Where(u => u.Role == "facilities" && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }
}
