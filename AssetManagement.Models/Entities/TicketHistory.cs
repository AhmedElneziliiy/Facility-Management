using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.Entities;

public class TicketHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TicketId { get; set; }

    [Required, MaxLength(50)]
    public string EventType { get; set; } = string.Empty; // created, status_changed, assigned, closed

    public string? CreatedByUserId { get; set; }

    public string? Details { get; set; } // JSON string

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
