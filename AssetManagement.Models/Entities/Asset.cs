using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.Entities;

public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string QRCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public Guid BuildingId { get; set; }

    public Guid? FloorId { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "active"; // active, in_repair, decommissioned

    [MaxLength(20)]
    public string Criticality { get; set; } = "low"; // safety, operational, low

    public DateTime? LastServicedAt { get; set; }

    public DateTime? NextServiceDueAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Building Building { get; set; } = null!;
    public Floor? Floor { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
