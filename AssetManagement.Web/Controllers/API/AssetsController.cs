using AssetManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.API;

[ApiController]
[Route("api/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    /// <summary>GET /api/assets/{qrCode} — look up asset by QR code (case-insensitive)</summary>
    [HttpGet("{qrCode}")]
    public async Task<IActionResult> GetAssetByQRCode(string qrCode)
    {
        var asset = await _assetService.GetAssetByQRCodeAsync(qrCode);
        if (asset == null)
            return NotFound(new { success = false, message = "Asset not found" });

        return Ok(new { success = true, data = asset });
    }

    /// <summary>GET /api/assets?buildingId={id} — list assets by building</summary>
    [HttpGet]
    public async Task<IActionResult> GetAssetsByBuilding([FromQuery] Guid? buildingId)
    {
        if (!buildingId.HasValue)
            return BadRequest(new { success = false, message = "buildingId query parameter is required" });

        var assets = await _assetService.GetAssetsByBuildingAsync(buildingId.Value);
        return Ok(new { success = true, data = assets });
    }
}
