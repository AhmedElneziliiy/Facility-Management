using System.Security.Claims;
using AssetManagement.Core.Interfaces;
using AssetManagement.Models.DTOs.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers.API;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>POST /api/tickets — create a new ticket</summary>
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        try
        {
            var ticket = await _ticketService.CreateTicketAsync(dto, userId);
            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id },
                new { success = true, data = ticket, message = "Ticket created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>GET /api/tickets — list tickets with optional filters</summary>
    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] TicketFilterDto filter)
    {
        var result = await _ticketService.GetTicketsAsync(filter);
        return Ok(new
        {
            success = true,
            data = result.Items,
            meta = new { total = result.Total, page = result.Page, pageSize = result.PageSize }
        });
    }

    /// <summary>GET /api/tickets/{id} — get full ticket details</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTicketById(Guid id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        if (ticket == null)
            return NotFound(new { success = false, message = "Ticket not found" });

        return Ok(new { success = true, data = ticket });
    }

    /// <summary>PUT /api/tickets/{id}/status — update ticket status (facilities only)</summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "facilities" && role != "manager")
            return StatusCode(403, new { success = false, message = "Only facilities team can update ticket status" });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        try
        {
            var ticket = await _ticketService.UpdateStatusAsync(id, dto, userId);
            if (ticket == null)
                return NotFound(new { success = false, message = "Ticket not found" });

            return Ok(new { success = true, data = ticket, message = "Status updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>PUT /api/tickets/{id}/close — close ticket with resolution notes (coordinator only)</summary>
    [HttpPut("{id:guid}/close")]
    public async Task<IActionResult> CloseTicket(Guid id, [FromBody] CloseTicketDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "coordinator" && role != "manager")
            return StatusCode(403, new { success = false, message = "Only coordinators can close tickets" });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        try
        {
            var ticket = await _ticketService.CloseTicketAsync(id, dto, userId);
            if (ticket == null)
                return NotFound(new { success = false, message = "Ticket not found" });

            return Ok(new { success = true, data = ticket, message = "Ticket closed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>POST /api/tickets/{id}/attachments — upload photo to ticket</summary>
    [HttpPost("{id:guid}/attachments")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file provided" });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        try
        {
            var attachment = await _ticketService.AddAttachmentAsync(id, file, userId);
            return StatusCode(201, new { success = true, data = attachment, message = "Attachment uploaded successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
