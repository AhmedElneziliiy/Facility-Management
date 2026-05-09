using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.Entities;

public class Floor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BuildingId { get; set; }

    public int Level { get; set; }

    [MaxLength(50)]
    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Building Building { get; set; } = null!;
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
