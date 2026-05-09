using System.ComponentModel.DataAnnotations;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.Entities;
using AssetManagement.Models.DTOs.Assets;

namespace AssetManagement.Models.ViewModels;

public class TicketDetailsViewModel
{
    public TicketDetailsDto Ticket { get; set; } = null!;
    public List<VendorDropdownDto> Vendors { get; set; } = new();
    public List<UserDropdownDto> FacilitiesUsers { get; set; } = new();
}

public class VendorDropdownDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserDropdownDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

// ── Create Ticket ────────────────────────────────────────────────────────────
public class AssetDropdownDto
{
    public Guid   Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string QRCode       { get; set; } = string.Empty;
}

public class FloorDropdownDto
{
    public Guid   Id    { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class CreateTicketViewModel
{
    [Required] public Guid   AssetId     { get; set; }
    [Required, MaxLength(200)] public string Title       { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required] public string Priority    { get; set; } = "normal";

    public List<AssetDropdownDto> Assets { get; set; } = new();
}

// ── User Management ──────────────────────────────────────────────────────────
public class CreateUserViewModel
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required] public string Username { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = "coordinator";
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
}

// ── Vendor Management ────────────────────────────────────────────────────────
public class CreateVendorViewModel
{
    [Required, MaxLength(100)] public string Name         { get; set; } = string.Empty;
    [MaxLength(100)]           public string? ContactName  { get; set; }
    [MaxLength(20)]            public string? ContactPhone { get; set; }
    [MaxLength(100), EmailAddress] public string? ContactEmail { get; set; }
}

// ── Asset Management ─────────────────────────────────────────────────────────
public class AssetFormViewModel
{
    public Guid?   Id             { get; set; }  // null = create, set = edit

    [Required, MaxLength(100)]
    public string  Name           { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string  QRCode         { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SerialNumber   { get; set; }

    [Required]
    public Guid    BuildingId     { get; set; }

    public Guid?   FloorId        { get; set; }

    [Required]
    public string  Status         { get; set; } = "operational";

    [Required]
    public string  Criticality    { get; set; } = "low";

    public DateTime? LastServicedAt    { get; set; }
    public DateTime? NextServiceDueAt  { get; set; }

    // Dropdowns
    public List<Building> Buildings { get; set; } = new();
    public List<Floor>    Floors    { get; set; } = new();
}

// ── Edit Vendor ──────────────────────────────────────────────────────────────
public class EditVendorViewModel
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)] public string  Name         { get; set; } = string.Empty;
    [MaxLength(100)]           public string? ContactName  { get; set; }
    [MaxLength(20)]            public string? ContactPhone { get; set; }
    [MaxLength(100), EmailAddress] public string? ContactEmail { get; set; }
}

// ── Edit User ────────────────────────────────────────────────────────────────
public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]                       public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress]         public string Email    { get; set; } = string.Empty;
    [Required]                       public string Role     { get; set; } = "coordinator";
}

// ── Building Management ───────────────────────────────────────────────────────
public class BuildingFormViewModel
{
    public Guid?   Id          { get; set; }

    [Required, MaxLength(100)] public string  Name        { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Address     { get; set; }
    public int FloorsCount { get; set; }
}

// ── Floor Management ──────────────────────────────────────────────────────────
public class FloorFormViewModel
{
    public Guid?  Id         { get; set; }
    public Guid   BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;

    [Required]
    public int    Level      { get; set; }

    [MaxLength(50)]
    public string? Name      { get; set; }
}

// ── Reports ──────────────────────────────────────────────────────────────────
public class ReportFilterViewModel
{
    public string? Status     { get; set; }
    public string? Priority   { get; set; }
    public Guid?   BuildingId { get; set; }
    public int     Page       { get; set; } = 1;
    public int     PageSize   { get; set; } = 50;
}

public class ReportsViewModel
{
    public List<TicketDto>  Tickets   { get; set; } = new();
    public int              Total     { get; set; }
    public int              Page      { get; set; }
    public int              PageSize  { get; set; }
    public ReportFilterViewModel Filter    { get; set; } = new();
    public List<Building>   Buildings { get; set; } = new();
}
