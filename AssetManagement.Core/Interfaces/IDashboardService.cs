using AssetManagement.Models.DTOs.Dashboard;

namespace AssetManagement.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? buildingId = null);
    Task<IEnumerable<BuildingPerformanceDto>> GetBuildingPerformanceAsync();
}
