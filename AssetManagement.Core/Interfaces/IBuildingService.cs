using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;

namespace AssetManagement.Core.Interfaces;

public interface IBuildingService
{
    Task<List<Building>> GetAllWithDetailsAsync();
    Task<Building?> GetByIdAsync(Guid id);
    Task<Building?> GetByIdWithDetailsAsync(Guid id);
    Task<Building> CreateAsync(BuildingFormViewModel model);
    Task<bool> UpdateAsync(Guid id, BuildingFormViewModel model);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
    Task<List<Building>> GetDropdownAsync();
    Task<List<Floor>> GetFloorsForBuildingAsync(Guid buildingId);
}
