using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;

namespace AssetManagement.Core.Interfaces;

public interface IVendorService
{
    Task<List<Vendor>> GetAllAsync();
    Task<Vendor?> GetByIdAsync(Guid id);
    Task<Vendor?> GetByIdWithTicketsAsync(Guid id);
    Task<Vendor> CreateAsync(CreateVendorViewModel model);
    Task<bool> UpdateAsync(Guid id, EditVendorViewModel model);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}
