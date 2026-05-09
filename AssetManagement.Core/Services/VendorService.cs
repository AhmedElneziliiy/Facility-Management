using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Services;

public class VendorService : IVendorService
{
    private readonly ApplicationDbContext _context;

    public VendorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vendor>> GetAllAsync()
    {
        return await _context.Vendors
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Vendor?> GetByIdAsync(Guid id)
    {
        return await _context.Vendors.FindAsync(id);
    }

    public async Task<Vendor?> GetByIdWithTicketsAsync(Guid id)
    {
        return await _context.Vendors
            .Include(v => v.Tickets)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vendor> CreateAsync(CreateVendorViewModel model)
    {
        var vendor = new Vendor
        {
            Name         = model.Name,
            ContactName  = model.ContactName,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();
        return vendor;
    }

    public async Task<bool> UpdateAsync(Guid id, EditVendorViewModel model)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) return false;

        vendor.Name         = model.Name;
        vendor.ContactName  = model.ContactName;
        vendor.ContactPhone = model.ContactPhone;
        vendor.ContactEmail = model.ContactEmail;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) return (false, null);

        if (await _context.Tickets.AnyAsync(t => t.AssignedVendorId == id))
            return (false, "Cannot delete vendor — they are assigned to existing tickets.");

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
