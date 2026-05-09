namespace AssetManagement.Models.DTOs.Tickets;

public class TicketFilterDto
{
    public string? CreatedByUserId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? BuildingId { get; set; }
    public string? AssignedToUserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
