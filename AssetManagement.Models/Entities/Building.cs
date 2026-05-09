using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.Entities;

public class Building
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    public int FloorsCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
