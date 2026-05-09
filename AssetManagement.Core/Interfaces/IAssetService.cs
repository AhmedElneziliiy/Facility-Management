using AssetManagement.Models.DTOs.Assets;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;

namespace AssetManagement.Core.Interfaces;

public interface IAssetService
{
    // Existing read methods
    Task<AssetDto?> GetAssetByQRCodeAsync(string qrCode);
    Task<IEnumerable<AssetDto>> GetAssetsByBuildingAsync(Guid buildingId);
    Task<PagedResult<AssetDto>> GetAssetsPagedAsync(Guid? buildingId, int page, int pageSize, string? status = null, string? criticality = null, string? search = null);

    // CRUD
    Task<Asset?> GetEntityByIdAsync(Guid id);
    Task<Asset?> GetEntityByIdWithNavAsync(Guid id);
    Task<Asset> CreateAsync(AssetFormViewModel model);
    Task<bool> UpdateAsync(Guid id, AssetFormViewModel model);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);

    // Validation
    Task<bool> IsQRCodeUniqueAsync(string qrCode, Guid? excludeId);

    // Dropdowns
    Task<AssetFormViewModel> BuildFormViewModelAsync(Asset? asset);
    Task<AssetFormViewModel> BuildFormViewModelForBuildingAsync(Guid buildingId);
    Task<List<AssetDropdownDto>> GetAssetDropdownAsync();
    Task<List<FloorDropdownDto>> GetFloorsForBuildingDropdownAsync(Guid buildingId);

    // Stats
    Task<(int Operational, int UnderMaintenance, int ServiceOverdue)> GetBuildingStatsAsync(Guid buildingId);
}
