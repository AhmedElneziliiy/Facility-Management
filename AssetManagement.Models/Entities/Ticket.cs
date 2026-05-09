using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.Entities;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(20)]
    public string TicketNumber { get; set; } = string.Empty;

    public Guid AssetId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Priority { get; set; } = "normal"; // critical, urgent, normal, low

    [MaxLength(20)]
    public string Status { get; set; } = "open"; // open, in_progress, closed, cancelled

    public int SLAHours { get; set; }

    public DateTime DueAt { get; set; }

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    public string? AssignedToUserId { get; set; }

    public Guid? AssignedVendorId { get; set; }

    public string? ResolutionNotes { get; set; }

    public string? ResolutionByUserId { get; set; }

    public decimal? ActualCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }

    // Navigation
    public Asset Asset { get; set; } = null!;
    public Vendor? AssignedVendor { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
}
