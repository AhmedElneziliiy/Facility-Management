using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.Entities;

public class Attachment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    [Required, MaxLength(255)]
    public string Filename { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ContentType { get; set; }

    public long SizeBytes { get; set; }

    public string? UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
