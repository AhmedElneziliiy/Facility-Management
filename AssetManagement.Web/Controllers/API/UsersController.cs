using System.Security.Claims;
using AssetManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.API;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public UsersController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    /// <summary>GET /api/users/me — get current authenticated user</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Invalid token" });

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "User not found" });

        return Ok(new { success = true, data = user });
    }

    /// <summary>GET /api/users/facilities — list all facilities team members (for assignment dropdown)</summary>
    [HttpGet("facilities")]
    public async Task<IActionResult> GetFacilitiesUsers()
    {
        var users = await _userService.GetFacilitiesUsersFullAsync();

        var data = users.Select(u => new
        {
            id       = u.Id,
            fullName = u.FullName,
            username = u.UserName,
            role     = u.Role
        });

        return Ok(new { success = true, data });
    }
}
