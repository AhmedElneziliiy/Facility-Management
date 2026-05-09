using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Models.DTOs.Assets;
using AssetManagement.Models.DTOs.Tickets;
using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Services;

public class AssetService : IAssetService
{
    private readonly ApplicationDbContext _context;

    public AssetService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── Existing read methods ─────────────────────────────────────────────────

    public async Task<AssetDto?> GetAssetByQRCodeAsync(string qrCode)
    {
        var asset = await _context.Assets
            .Include(a => a.Building)
            .Include(a => a.Floor)
            .FirstOrDefaultAsync(a => a.QRCode.ToLower() == qrCode.ToLower());

        if (asset == null) return null;

        return MapToDto(asset);
    }

    public async Task<IEnumerable<AssetDto>> GetAssetsByBuildingAsync(Guid buildingId)
    {
        return await _context.Assets
            .Include(a => a.Building)
            .Include(a => a.Floor)
            .Where(a => a.BuildingId == buildingId)
            .Select(a => new AssetDto
            {
                Id               = a.Id,
                Name             = a.Name,
                QRCode           = a.QRCode,
                SerialNumber     = a.SerialNumber,
                BuildingId       = a.BuildingId,
                BuildingName     = a.Building.Name,
                FloorId          = a.FloorId,
                FloorLevel       = a.Floor != null ? a.Floor.Level : (int?)null,
                Status           = a.Status,
                Criticality      = a.Criticality,
                LastServicedAt   = a.LastServicedAt,
                NextServiceDueAt = a.NextServiceDueAt
            })
            .ToListAsync();
    }

    public async Task<PagedResult<AssetDto>> GetAssetsPagedAsync(Guid? buildingId, int page, int pageSize, string? status = null, string? criticality = null, string? search = null)
    {
        var query = _context.Assets
            .Include(a => a.Building)
            .Include(a => a.Floor)
            .AsQueryable();

        if (buildingId.HasValue)
            query = query.Where(a => a.BuildingId == buildingId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        if (!string.IsNullOrWhiteSpace(criticality))
            query = query.Where(a => a.Criticality == criticality);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(s) ||
                a.QRCode.ToLower().Contains(s) ||
                (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(s)));
        }

        query = query.OrderBy(a => a.Name);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AssetDto
            {
                Id               = a.Id,
                Name             = a.Name,
                QRCode           = a.QRCode,
                SerialNumber     = a.SerialNumber,
                BuildingId       = a.BuildingId,
                BuildingName     = a.Building.Name,
                FloorId          = a.FloorId,
                FloorLevel       = a.Floor != null ? a.Floor.Level : (int?)null,
                Status           = a.Status,
                Criticality      = a.Criticality,
                LastServicedAt   = a.LastServicedAt,
                NextServiceDueAt = a.NextServiceDueAt
            })
            .ToListAsync();

        return new PagedResult<AssetDto>
        {
            Items    = items,
            Total    = total,
            Page     = page,
            PageSize = pageSize
        };
    }

    // ── CRUD ─────────────────────────────────────────────────────────────────

    public async Task<Asset?> GetEntityByIdAsync(Guid id)
    {
        return await _context.Assets.FindAsync(id);
    }

    public async Task<Asset?> GetEntityByIdWithNavAsync(Guid id)
    {
        return await _context.Assets
            .Include(a => a.Building)
            .Include(a => a.Floor)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Asset> CreateAsync(AssetFormViewModel model)
    {
        var asset = new Asset
        {
            Name             = model.Name,
            QRCode           = model.QRCode.Trim().ToUpper(),
            SerialNumber     = model.SerialNumber,
            BuildingId       = model.BuildingId,
            FloorId          = model.FloorId,
            Status           = model.Status,
            Criticality      = model.Criticality,
            LastServicedAt   = model.LastServicedAt,
            NextServiceDueAt = model.NextServiceDueAt
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task<bool> UpdateAsync(Guid id, AssetFormViewModel model)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return false;

        asset.Name             = model.Name;
        asset.QRCode           = model.QRCode.Trim().ToUpper();
        asset.SerialNumber     = model.SerialNumber;
        asset.BuildingId       = model.BuildingId;
        asset.FloorId          = model.FloorId;
        asset.Status           = model.Status;
        asset.Criticality      = model.Criticality;
        asset.LastServicedAt   = model.LastServicedAt;
        asset.NextServiceDueAt = model.NextServiceDueAt;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return (false, null);

        if (await _context.Tickets.AnyAsync(t => t.AssetId == id))
            return (false, "Cannot delete — asset has linked tickets.");

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    public async Task<bool> IsQRCodeUniqueAsync(string qrCode, Guid? excludeId)
    {
        var qr = qrCode.Trim().ToUpper();
        return !await _context.Assets.AnyAsync(a =>
            a.QRCode.ToUpper() == qr && a.Id != (excludeId ?? Guid.Empty));
    }

    // ── Dropdowns ────────────────────────────────────────────────────────────

    public async Task<AssetFormViewModel> BuildFormViewModelAsync(Asset? asset)
    {
        var buildings  = await _context.Buildings.OrderBy(b => b.Name).ToListAsync();
        var buildingId = asset?.BuildingId ?? (buildings.Count > 0 ? buildings[0].Id : Guid.Empty);
        var floors     = await _context.Floors
            .Where(f => f.BuildingId == buildingId)
            .OrderBy(f => f.Level)
            .ToListAsync();

        return new AssetFormViewModel
        {
            Id               = asset?.Id,
            Name             = asset?.Name             ?? string.Empty,
            QRCode           = asset?.QRCode           ?? string.Empty,
            SerialNumber     = asset?.SerialNumber,
            BuildingId       = buildingId,
            FloorId          = asset?.FloorId,
            Status           = asset?.Status           ?? "operational",
            Criticality      = asset?.Criticality      ?? "low",
            LastServicedAt   = asset?.LastServicedAt,
            NextServiceDueAt = asset?.NextServiceDueAt,
            Buildings        = buildings,
            Floors           = floors
        };
    }

    public async Task<AssetFormViewModel> BuildFormViewModelForBuildingAsync(Guid buildingId)
    {
        var buildings = await _context.Buildings.OrderBy(b => b.Name).ToListAsync();
        var floors    = await _context.Floors
            .Where(f => f.BuildingId == buildingId)
            .OrderBy(f => f.Level)
            .ToListAsync();

        return new AssetFormViewModel
        {
            BuildingId  = buildingId,
            Buildings   = buildings,
            Floors      = floors,
            Status      = "operational",
            Criticality = "low"
        };
    }

    public async Task<List<AssetDropdownDto>> GetAssetDropdownAsync()
    {
        return await _context.Assets
            .OrderBy(a => a.Name)
            .Select(a => new AssetDropdownDto
            {
                Id           = a.Id,
                Name         = a.Name,
                BuildingName = a.Building.Name,
                QRCode       = a.QRCode
            })
            .ToListAsync();
    }

    public async Task<List<FloorDropdownDto>> GetFloorsForBuildingDropdownAsync(Guid buildingId)
    {
        return await _context.Floors
            .Where(f => f.BuildingId == buildingId)
            .OrderBy(f => f.Level)
            .Select(f => new FloorDropdownDto
            {
                Id    = f.Id,
                Label = f.Name != null ? $"Floor {f.Level} – {f.Name}" : $"Floor {f.Level}"
            })
            .ToListAsync();
    }

    // ── Stats ─────────────────────────────────────────────────────────────────

    public async Task<(int Operational, int UnderMaintenance, int ServiceOverdue)> GetBuildingStatsAsync(Guid buildingId)
    {
        var all = await _context.Assets
            .Where(a => a.BuildingId == buildingId)
            .ToListAsync();

        var operational = all.Count(a => a.Status == "operational");
        var underMaint  = all.Count(a => a.Status == "under_maintenance");
        var svcOverdue  = all.Count(a => a.NextServiceDueAt.HasValue && a.NextServiceDueAt.Value < DateTime.UtcNow);

        return (operational, underMaint, svcOverdue);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static AssetDto MapToDto(Asset asset) => new AssetDto
    {
        Id               = asset.Id,
        Name             = asset.Name,
        QRCode           = asset.QRCode,
        SerialNumber     = asset.SerialNumber,
        BuildingId       = asset.BuildingId,
        BuildingName     = asset.Building.Name,
        FloorId          = asset.FloorId,
        FloorLevel       = asset.Floor?.Level,
        Status           = asset.Status,
        Criticality      = asset.Criticality,
        LastServicedAt   = asset.LastServicedAt,
        NextServiceDueAt = asset.NextServiceDueAt
    };
}
