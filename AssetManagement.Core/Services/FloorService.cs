using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Services;

public class FloorService : IFloorService
{
    private readonly ApplicationDbContext _context;

    public FloorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Floor>> GetAllWithBuildingAsync()
    {
        return await _context.Floors
            .Include(f => f.Building)
            .OrderBy(f => f.Building.Name).ThenBy(f => f.Level)
            .ToListAsync();
    }

    public async Task<Floor?> GetByIdAsync(Guid id)
    {
        return await _context.Floors.FindAsync(id);
    }

    public async Task<Floor?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Floors
            .Include(f => f.Building)
            .Include(f => f.Assets)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Floor> CreateAsync(FloorFormViewModel model)
    {
        var floor = new Floor
        {
            BuildingId = model.BuildingId,
            Level      = model.Level,
            Name       = model.Name
        };

        _context.Floors.Add(floor);
        await _context.SaveChangesAsync();
        return floor;
    }

    public async Task<bool> UpdateAsync(Guid id, FloorFormViewModel model)
    {
        var floor = await _context.Floors.FindAsync(id);
        if (floor == null) return false;

        floor.Level = model.Level;
        floor.Name  = model.Name;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
        var floor = await _context.Floors.FindAsync(id);
        if (floor == null) return (false, null);

        if (await _context.Assets.AnyAsync(a => a.FloorId == id))
            return (false, "Cannot delete floor — it has assigned assets.");

        _context.Floors.Remove(floor);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> IsLevelUniqueAsync(Guid buildingId, int level, Guid? excludeId)
    {
        if (excludeId.HasValue)
        {
            var excl = excludeId.Value;
            return !await _context.Floors.AnyAsync(f =>
                f.BuildingId == buildingId &&
                f.Level == level &&
                f.Id != excl);
        }

        return !await _context.Floors.AnyAsync(f =>
            f.BuildingId == buildingId &&
            f.Level == level);
    }
}
