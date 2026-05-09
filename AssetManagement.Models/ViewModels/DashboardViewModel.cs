using AssetManagement.Models.DTOs.Dashboard;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.Entities;

namespace AssetManagement.Models.ViewModels;

public class DashboardViewModel
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<BuildingPerformanceDto> BuildingPerformance { get; set; } = new();
    public List<TicketDto> RecentTickets { get; set; } = new();
    public List<Building> Buildings { get; set; } = new();
    public Guid? SelectedBuildingId { get; set; }
}
