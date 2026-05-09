namespace AssetManagement.Models.DTOs.Tickets;

public class TicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public int? FloorLevel { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime DueAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string? AssignedToUserName { get; set; }
    public bool IsOverdue => DateTime.UtcNow > DueAt && Status != "closed";
}

public class TicketDetailsDto : TicketDto
{
    public string Description { get; set; } = string.Empty;
    public int SLAHours { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
    public Guid? AssignedVendorId { get; set; }
    public string? AssignedVendorName { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? ResolutionByUserId { get; set; }
    public string? ResolutionByUserName { get; set; }
    public int? TimeSpentMinutes { get; set; }
    public decimal? ActualCost { get; set; }
    public AssetSnapshotDto AssetSnapshot { get; set; } = null!;
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<TicketHistoryDto> History { get; set; } = new();
}

public class AssetSnapshotDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int? FloorLevel { get; set; }
    public string Criticality { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class TicketHistoryDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? Details { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
