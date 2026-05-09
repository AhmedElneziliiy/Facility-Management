using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Models.Entities;
using AssetManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Services;

public class BuildingService : IBuildingService
{
    private readonly ApplicationDbContext _context;

    public BuildingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Building>> GetAllWithDetailsAsync()
    {
        return await _context.Buildings
            .Include(b => b.Floors)
            .Include(b => b.Assets)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<Building?> GetByIdAsync(Guid id)
    {
        return await _context.Buildings.FindAsync(id);
    }

    public async Task<Building?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Buildings
            .Include(b => b.Floors.OrderBy(f => f.Level))
            .Include(b => b.Assets)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Building> CreateAsync(BuildingFormViewModel model)
    {
        var building = new Building
        {
            Name        = model.Name,
            Address     = model.Address,
            FloorsCount = model.FloorsCount
        };

        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();
        return building;
    }

    public async Task<bool> UpdateAsync(Guid id, BuildingFormViewModel model)
    {
        var building = await _context.Buildings.FindAsync(id);
        if (building == null) return false;

        building.Name        = model.Name;
        building.Address     = model.Address;
        building.FloorsCount = model.FloorsCount;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id)
    {
        var building = await _context.Buildings.FindAsync(id);
        if (building == null) return (false, null);

        if (await _context.Assets.AnyAsync(a => a.BuildingId == id))
            return (false, "Cannot delete — building has linked assets.");

        if (await _context.Floors.AnyAsync(f => f.BuildingId == id))
            return (false, "Cannot delete — building has floors. Delete floors first.");

        _context.Buildings.Remove(building);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<Building>> GetDropdownAsync()
    {
        return await _context.Buildings
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<List<Floor>> GetFloorsForBuildingAsync(Guid buildingId)
    {
        return await _context.Floors
            .Where(f => f.BuildingId == buildingId)
            .OrderBy(f => f.Level)
            .ToListAsync();
    }

    public async Task<bool> HasAssetsAsync(Guid id)
    {
        return await _context.Assets.AnyAsync(a => a.BuildingId == id);
    }

    public async Task<bool> HasFloorsAsync(Guid id)
    {
        return await _context.Floors.AnyAsync(f => f.BuildingId == id);
    }
}
