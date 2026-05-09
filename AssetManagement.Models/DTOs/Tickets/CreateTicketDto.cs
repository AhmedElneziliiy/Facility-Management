using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.DTOs.Tickets;

public class CreateTicketDto
{
    [Required]
    public Guid AssetId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = "normal"; // critical, urgent, normal, low

    public List<CreateAttachmentDto> Attachments { get; set; } = new();
}

public class CreateAttachmentDto
{
    public string Filename { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
}
