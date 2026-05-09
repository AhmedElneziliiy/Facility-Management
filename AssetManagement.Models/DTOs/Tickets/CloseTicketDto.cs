using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.DTOs.Tickets;

public class CloseTicketDto
{
    [Required]
    public string ResolutionNotes { get; set; } = string.Empty;

    public decimal? ActualCost { get; set; }
}
