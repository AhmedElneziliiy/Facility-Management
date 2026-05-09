namespace AssetManagement.Models.DTOs.Dashboard;

public class BuildingPerformanceDto
{
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public int OpenTickets { get; set; }
    public int CriticalTickets { get; set; }
    public double AvgResolutionTimeMinutes { get; set; }
}
