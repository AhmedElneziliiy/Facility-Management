using AssetManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.API;

[ApiController]
[Route("api/vendors")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    /// <summary>GET /api/vendors — list all vendors</summary>
    [HttpGet]
    public async Task<IActionResult> GetVendors()
    {
        var vendors = await _vendorService.GetAllAsync();

        var data = vendors.Select(v => new
        {
            id           = v.Id,
            name         = v.Name,
            contactName  = v.ContactName,
            contactPhone = v.ContactPhone,
            contactEmail = v.ContactEmail
        });

        return Ok(new { success = true, data });
    }
}
