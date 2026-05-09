using AssetManagement.Infrastructure.Identity;
using AssetManagement.Models.DTOs.Auth;

namespace AssetManagement.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<UserDto?> GetCurrentUserAsync(string userId);
    string GenerateJwtToken(ApplicationUser user);
}
