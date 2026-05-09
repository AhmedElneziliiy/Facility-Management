using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.DTOs.Assets;

namespace AssetManagement.Models.ViewModels;

public class TicketListViewModel
{
    public List<TicketDto> Tickets { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public TicketFilterDto Filter { get; set; } = new();
}
