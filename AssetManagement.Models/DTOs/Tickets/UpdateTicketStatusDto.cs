using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.DTOs.Tickets;

public class UpdateTicketStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty; // in_progress

    public string? AssignedToUserId { get; set; }

    public Guid? AssignedVendorId { get; set; }
}
