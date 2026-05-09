using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;

namespace AssetManagement.Core.Interfaces;

public interface IFloorService
{
    Task<List<Floor>> GetAllWithBuildingAsync();
    Task<Floor?> GetByIdAsync(Guid id);
    Task<Floor?> GetByIdWithDetailsAsync(Guid id);
    Task<Floor> CreateAsync(FloorFormViewModel model);
    Task<bool> UpdateAsync(Guid id, FloorFormViewModel model);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
    Task<bool> IsLevelUniqueAsync(Guid buildingId, int level, Guid? excludeId);
}
