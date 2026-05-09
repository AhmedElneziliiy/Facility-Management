using AssetManagement.Infrastructure.Identity;
using AssetManagement.Models.ViewModels;

namespace AssetManagement.Core.Interfaces;

public interface IUserService
{
    Task<List<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<(bool Success, string? Error, string? UserId)> CreateAsync(CreateUserViewModel model);
    Task<(bool Success, string? Error)> UpdateAsync(string id, EditUserViewModel model);
    Task<bool> ToggleActiveAsync(string id);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string id, string newPassword);
    Task<(bool Success, string? Error)> DeleteAsync(string id);
    Task<int> GetTicketsCreatedCountAsync(string userId);
    Task<int> GetTicketsAssignedCountAsync(string userId);
    Task<List<UserDropdownDto>> GetFacilitiesUsersAsync();
    Task<List<ApplicationUser>> GetFacilitiesUsersFullAsync();
    Task<bool> UsernameExistsAsync(string username);
}
