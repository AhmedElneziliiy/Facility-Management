using System.Text.Json;
using AssetManagement.Core.Helpers;
using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Identity;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketService(ApplicationDbContext context, IFileStorageService fileStorage, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _fileStorage = fileStorage;
        _userManager = userManager;
    }

    public async Task<TicketDetailsDto> CreateTicketAsync(CreateTicketDto dto, string userId)
    {
        var asset = await _context.Assets
            .Include(a => a.Building)
            .Include(a => a.Floor)
            .FirstOrDefaultAsync(a => a.Id == dto.AssetId)
            ?? throw new InvalidOperationException("Asset not found.");

        var ticketNumber = await TicketNumberGenerator.GenerateAsync(_context);
        var slaHours = SLACalculator.GetSLAHours(dto.Priority);
        var createdAt = DateTime.UtcNow;

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            AssetId = dto.AssetId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority.ToLower(),
            Status = "open",
            SLAHours = slaHours,
            DueAt = SLACalculator.CalculateDueDate(createdAt, slaHours),
            CreatedByUserId = userId,
            CreatedAt = createdAt
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // Attachments from DTO (URLs already stored by client or pre-upload)
        foreach (var att in dto.Attachments)
        {
            _context.Attachments.Add(new Attachment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                Filename = att.Filename,
                Url = att.Url,
                ContentType = att.ContentType,
                SizeBytes = att.SizeBytes,
                UploadedByUserId = userId,
                UploadedAt = createdAt
            });
        }

        // History entry
        _context.TicketHistory.Add(new TicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            EventType = "created",
            CreatedByUserId = userId,
            Details = $"Ticket created with priority: {dto.Priority.ToLower()}",
            CreatedAt = createdAt
        });

        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(ticket.Id) ?? throw new InvalidOperationException("Ticket not found after creation.");
    }

    public async Task<TicketDetailsDto?> GetTicketByIdAsync(Guid id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Asset).ThenInclude(a => a.Building)
            .Include(t => t.Asset).ThenInclude(a => a.Floor)
            .Include(t => t.AssignedVendor)
            .Include(t => t.Attachments)
            .Include(t => t.History)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return null;

        // Collect all user IDs needed for this ticket in one DB round-trip
        var allUserIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(ticket.CreatedByUserId))      allUserIds.Add(ticket.CreatedByUserId);
        if (!string.IsNullOrEmpty(ticket.AssignedToUserId))     allUserIds.Add(ticket.AssignedToUserId);
        if (!string.IsNullOrEmpty(ticket.ResolutionByUserId))   allUserIds.Add(ticket.ResolutionByUserId);
        foreach (var h in ticket.History)
            if (!string.IsNullOrEmpty(h.CreatedByUserId))       allUserIds.Add(h.CreatedByUserId);

        var userMap = await _context.Users
            .Where(u => allUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, StringComparer.OrdinalIgnoreCase);

        string? LookupName(string? uid) => uid != null && userMap.TryGetValue(uid, out var n) ? n : "(deleted user)";

        var createdByUser    = ticket.CreatedByUserId    != null ? LookupName(ticket.CreatedByUserId)    : null;
        var assignedToUser   = ticket.AssignedToUserId   != null ? LookupName(ticket.AssignedToUserId)   : null;
        var resolvedByUser   = ticket.ResolutionByUserId != null ? LookupName(ticket.ResolutionByUserId) : null;

        var historyDtos = new List<TicketHistoryDto>();
        foreach (var h in ticket.History.OrderBy(h => h.CreatedAt))
        {
            var histUser = h.CreatedByUserId != null ? LookupName(h.CreatedByUserId) : null;
            historyDtos.Add(new TicketHistoryDto
            {
                Id = h.Id,
                EventType = h.EventType,
                CreatedAt = h.CreatedAt,
                CreatedByUserId = h.CreatedByUserId,
                CreatedByUserName = histUser,
                Details = h.Details
            });
        }

        return new TicketDetailsDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            AssetId = ticket.AssetId,
            AssetName = ticket.Asset.Name,
            BuildingId = ticket.Asset.BuildingId,
            BuildingName = ticket.Asset.Building.Name,
            FloorLevel = ticket.Asset.Floor?.Level,
            Priority = ticket.Priority,
            Status = ticket.Status,
            Criticality = ticket.Asset.Criticality,
            SLAHours = ticket.SLAHours,
            DueAt = ticket.DueAt,
            CreatedAt = ticket.CreatedAt,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = createdByUser,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = assignedToUser,
            AssignedVendorId = ticket.AssignedVendorId,
            AssignedVendorName = ticket.AssignedVendor?.Name,
            ClosedAt = ticket.ClosedAt,
            ResolutionNotes = ticket.ResolutionNotes,
            ResolutionByUserId = ticket.ResolutionByUserId,
            ResolutionByUserName = resolvedByUser,
            TimeSpentMinutes = ticket.ClosedAt.HasValue
                ? (int)(ticket.ClosedAt.Value - ticket.CreatedAt).TotalMinutes
                : null,
            ActualCost = ticket.ActualCost,
            AssetSnapshot = new AssetSnapshotDto
            {
                Id = ticket.Asset.Id,
                Name = ticket.Asset.Name,
                BuildingName = ticket.Asset.Building.Name,
                FloorLevel = ticket.Asset.Floor?.Level,
                Criticality = ticket.Asset.Criticality,
                SerialNumber = ticket.Asset.SerialNumber
            },
            Attachments = ticket.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                TicketId = a.TicketId,
                Filename = a.Filename,
                Url = a.Url,
                ContentType = a.ContentType,
                SizeBytes = a.SizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList(),
            History = historyDtos
        };
    }

    public async Task<PagedResult<TicketDto>> GetTicketsAsync(TicketFilterDto filter)
    {
        var query = _context.Tickets
            .Include(t => t.Asset).ThenInclude(a => a.Building)
            .Include(t => t.Asset).ThenInclude(a => a.Floor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.CreatedByUserId))
            query = query.Where(t => t.CreatedByUserId == filter.CreatedByUserId);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(t => t.Status == filter.Status.ToLower());

        if (!string.IsNullOrEmpty(filter.Priority))
            query = query.Where(t => t.Priority == filter.Priority.ToLower());

        if (filter.BuildingId.HasValue)
            query = query.Where(t => t.Asset.BuildingId == filter.BuildingId.Value);

        if (!string.IsNullOrEmpty(filter.AssignedToUserId))
            query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);

        var total = await query.CountAsync();

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Fetch usernames — single query instead of N _userManager round-trips
        var userIds = tickets
            .SelectMany(t => new[] { t.CreatedByUserId, t.AssignedToUserId })
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, StringComparer.OrdinalIgnoreCase);

        return new PagedResult<TicketDto>
        {
            Items = tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Title = t.Title,
                AssetId = t.AssetId,
                AssetName = t.Asset.Name,
                BuildingId = t.Asset.BuildingId,
                BuildingName = t.Asset.Building.Name,
                FloorLevel = t.Asset.Floor?.Level,
                Priority = t.Priority,
                Status = t.Status,
                Criticality = t.Asset.Criticality,
                CreatedAt = t.CreatedAt,
                DueAt = t.DueAt,
                CreatedByUserName = users.GetValueOrDefault(t.CreatedByUserId, "(deleted user)"),
                AssignedToUserName = t.AssignedToUserId != null ? users.GetValueOrDefault(t.AssignedToUserId, "(deleted user)") : null
            }).ToList(),
            Total = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<TicketDetailsDto?> UpdateStatusAsync(Guid id, UpdateTicketStatusDto dto, string userId)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return null;

        var newStatus = dto.Status.ToLower();
        var statusChanging = newStatus != ticket.Status;

        // Only validate the transition when the status is actually changing
        if (statusChanging && !IsValidStatusTransition(ticket.Status, newStatus))
            throw new InvalidOperationException($"Cannot transition from '{ticket.Status}' to '{newStatus}'.");

        var oldStatus = ticket.Status;

        if (statusChanging)
            ticket.Status = newStatus;

        // Resolve assignment changes
        bool userAssignmentChanged = dto.AssignedToUserId != null && dto.AssignedToUserId != ticket.AssignedToUserId;
        bool vendorAssignmentChanged = dto.AssignedVendorId.HasValue && dto.AssignedVendorId != ticket.AssignedVendorId;

        if (userAssignmentChanged)
        {
            ticket.AssignedToUserId = dto.AssignedToUserId;
        }

        if (vendorAssignmentChanged)
            ticket.AssignedVendorId = dto.AssignedVendorId;

        // Build a single human-readable "assigned" history entry if anything was re-assigned
        if (userAssignmentChanged || vendorAssignmentChanged)
        {
            var assignedUserName = userAssignmentChanged && dto.AssignedToUserId != null
                ? await _context.Users.Where(u => u.Id == dto.AssignedToUserId).Select(u => u.FullName).FirstOrDefaultAsync()
                : null;
            var assignedVendorName = vendorAssignmentChanged && dto.AssignedVendorId.HasValue
                ? await _context.Vendors.Where(v => v.Id == dto.AssignedVendorId.Value).Select(v => v.Name).FirstOrDefaultAsync()
                : null;

            var parts = new List<string>();
            if (assignedUserName   != null) parts.Add($"Assigned to {assignedUserName}");
            if (assignedVendorName != null) parts.Add($"Vendor: {assignedVendorName}");

            _context.TicketHistory.Add(new TicketHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                EventType = "assigned",
                CreatedByUserId = userId,
                Details = string.Join(". ", parts),
                CreatedAt = DateTime.UtcNow
            });
        }

        // Only log a status_changed entry when the status actually changed
        if (statusChanging)
        {
            _context.TicketHistory.Add(new TicketHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                EventType = "status_changed",
                CreatedByUserId = userId,
                Details = $"Status changed from {oldStatus} to {newStatus}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return await GetTicketByIdAsync(id);
    }

    public async Task<TicketDetailsDto?> CloseTicketAsync(Guid id, CloseTicketDto dto, string userId)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return null;

        if (ticket.Status == "closed")
            throw new InvalidOperationException("Ticket is already closed.");

        var oldStatus = ticket.Status;
        ticket.Status = "closed";
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.ResolutionNotes = dto.ResolutionNotes;
        ticket.ResolutionByUserId = userId;
        ticket.ActualCost = dto.ActualCost;

        var timeSpent = (int)(ticket.ClosedAt!.Value - ticket.CreatedAt).TotalMinutes;
        _context.TicketHistory.Add(new TicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            EventType = "closed",
            CreatedByUserId = userId,
            Details = $"Resolved in {timeSpent} min. Notes: {dto.ResolutionNotes}",
            CreatedAt = ticket.ClosedAt.Value
        });

        await _context.SaveChangesAsync();
        return await GetTicketByIdAsync(id);
    }

    public async Task<AttachmentDto> AddAttachmentAsync(Guid ticketId, IFormFile file, string userId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId)
            ?? throw new InvalidOperationException("Ticket not found.");

        var url = await _fileStorage.SaveFileAsync(file, $"tickets/{ticketId}");

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            Filename = file.FileName,
            Url = url,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedByUserId = userId,
            UploadedAt = DateTime.UtcNow
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        return new AttachmentDto
        {
            Id = attachment.Id,
            TicketId = attachment.TicketId,
            Filename = attachment.Filename,
            Url = attachment.Url,
            ContentType = attachment.ContentType,
            SizeBytes = attachment.SizeBytes,
            UploadedAt = attachment.UploadedAt
        };
    }

    private static bool IsValidStatusTransition(string current, string target)
    {
        var validTransitions = new Dictionary<string, HashSet<string>>
        {
            { "open",        new() { "in_progress", "cancelled" } },
            { "in_progress", new() { "closed", "open" } },
            { "closed",      new() { } },
            { "cancelled",   new() { } }
        };

        return validTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target.ToLower());
    }
}
