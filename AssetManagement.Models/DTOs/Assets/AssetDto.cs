namespace AssetManagement.Models.DTOs.Assets;

public class AssetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public Guid? FloorId { get; set; }
    public int? FloorLevel { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
    public DateTime? LastServicedAt { get; set; }
    public DateTime? NextServiceDueAt { get; set; }
}
