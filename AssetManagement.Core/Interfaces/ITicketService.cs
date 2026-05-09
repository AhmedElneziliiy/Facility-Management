using AssetManagement.Models.DTOs.Tickets;
using Microsoft.AspNetCore.Http;

namespace AssetManagement.Core.Interfaces;

public interface ITicketService
{
    Task<TicketDetailsDto> CreateTicketAsync(CreateTicketDto dto, string userId);
    Task<TicketDetailsDto?> GetTicketByIdAsync(Guid id);
    Task<PagedResult<TicketDto>> GetTicketsAsync(TicketFilterDto filter);
    Task<TicketDetailsDto?> UpdateStatusAsync(Guid id, UpdateTicketStatusDto dto, string userId);
    Task<TicketDetailsDto?> CloseTicketAsync(Guid id, CloseTicketDto dto, string userId);
    Task<AttachmentDto> AddAttachmentAsync(Guid ticketId, IFormFile file, string userId);
}
