using AssetManagement.Core.Interfaces;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Models.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? buildingId = null)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var assetsQuery  = _context.Assets.AsQueryable();
        var ticketsQuery = _context.Tickets.AsQueryable();

        if (buildingId.HasValue)
        {
            assetsQuery  = assetsQuery.Where(a => a.BuildingId == buildingId.Value);
            ticketsQuery = ticketsQuery.Where(t => t.Asset.BuildingId == buildingId.Value);
        }

        // ── Assets ──────────────────────────────────────────────────────
        var totalAssets             = await assetsQuery.CountAsync();
        var assetsOperational       = await assetsQuery.CountAsync(a => a.Status == "operational");
        var assetsUnderMaintenance  = await assetsQuery.CountAsync(a => a.Status == "under_maintenance");

        // ── Tickets — load once ──────────────────────────────────────────
        var tickets = await ticketsQuery.ToListAsync();

        var openTickets       = tickets.Count(t => t.Status == "open");
        var inProgressTickets = tickets.Count(t => t.Status == "in_progress");
        var closedTickets     = tickets.Count(t => t.Status == "closed");
        var criticalTickets   = tickets.Count(t => t.Priority == "critical" && t.Status != "closed" && t.Status != "cancelled");
        var overdueTickets    = tickets.Count(t => t.Status != "closed" && t.Status != "cancelled" && t.DueAt < now);

        // ── This month ───────────────────────────────────────────────────
        var createdThisMonth  = tickets.Count(t => t.CreatedAt >= monthStart);
        var resolvedThisMonth = tickets.Count(t => t.Status == "closed" && t.ClosedAt.HasValue && t.ClosedAt.Value >= monthStart);
        var overdueThisMonth  = tickets.Count(t => t.CreatedAt >= monthStart && t.Status != "closed" && t.DueAt < now);

        // ── Resolution performance ───────────────────────────────────────
        var closed = tickets.Where(t => t.Status == "closed" && t.ClosedAt.HasValue).ToList();
        var avgResolution = closed.Any()
            ? closed.Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalMinutes)
            : 0;

        // SLA compliance = % of closed tickets that were closed before DueAt
        var slaCompliant = closed.Count(t => t.ClosedAt!.Value <= t.DueAt);
        var slaPercent   = closed.Any() ? Math.Round((double)slaCompliant / closed.Count * 100, 1) : 0;

        // ── Last 7 days trend ────────────────────────────────────────────
        var trend = new List<TrendPointDto>();
        for (int i = 6; i >= 0; i--)
        {
            var day      = now.Date.AddDays(-i);
            var dayEnd   = day.AddDays(1);
            var dayUtc   = DateTime.SpecifyKind(day, DateTimeKind.Utc);
            var dayEndUtc = DateTime.SpecifyKind(dayEnd, DateTimeKind.Utc);
            trend.Add(new TrendPointDto
            {
                Label    = day.ToString("ddd"),
                Created  = tickets.Count(t => t.CreatedAt  >= dayUtc && t.CreatedAt  < dayEndUtc),
                Resolved = tickets.Count(t => t.ClosedAt.HasValue && t.ClosedAt.Value >= dayUtc && t.ClosedAt.Value < dayEndUtc)
            });
        }

        // ── Monthly resolution — last 6 months ──────────────────────────
        var monthly = new List<MonthlyResolutionDto>();
        for (int i = 5; i >= 0; i--)
        {
            var mStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            var mEnd   = mStart.AddMonths(1);
            monthly.Add(new MonthlyResolutionDto
            {
                Month    = mStart.ToString("MMM yy"),
                Created  = tickets.Count(t => t.CreatedAt >= mStart && t.CreatedAt < mEnd),
                Resolved = tickets.Count(t => t.ClosedAt.HasValue && t.ClosedAt.Value >= mStart && t.ClosedAt.Value < mEnd)
            });
        }

        return new DashboardStatsDto
        {
            TotalAssets            = totalAssets,
            AssetsOperational      = assetsOperational,
            AssetsUnderMaintenance = assetsUnderMaintenance,
            TotalTickets           = tickets.Count,
            OpenTickets            = openTickets,
            InProgressTickets      = inProgressTickets,
            ClosedTickets          = closedTickets,
            CriticalTickets        = criticalTickets,
            OverdueTickets         = overdueTickets,
            CreatedThisMonth       = createdThisMonth,
            ResolvedThisMonth      = resolvedThisMonth,
            OverdueThisMonth       = overdueThisMonth,
            AvgResolutionTimeMinutes = Math.Round(avgResolution, 1),
            SLACompliancePercent   = slaPercent,
            Last7DaysTrend         = trend,
            TicketsByPriority = new TicketsByPriorityDto
            {
                Critical = tickets.Count(t => t.Priority == "critical"),
                Urgent   = tickets.Count(t => t.Priority == "urgent"),
                Normal   = tickets.Count(t => t.Priority == "normal"),
                Low      = tickets.Count(t => t.Priority == "low")
            },
            TicketsByStatus = new TicketsByStatusDto
            {
                Open       = openTickets,
                InProgress = inProgressTickets,
                Closed     = closedTickets
            },
            MonthlyResolution = monthly
        };
    }

    public async Task<IEnumerable<BuildingPerformanceDto>> GetBuildingPerformanceAsync()
    {
        var buildings = await _context.Buildings.ToListAsync();
        var result = new List<BuildingPerformanceDto>();

        foreach (var building in buildings)
        {
            var tickets = await _context.Tickets
                .Where(t => t.Asset.BuildingId == building.Id)
                .ToListAsync();

            var openTickets     = tickets.Count(t => t.Status != "closed" && t.Status != "cancelled");
            var criticalTickets = tickets.Count(t => t.Priority == "critical" && t.Status != "closed");
            var closed          = tickets.Where(t => t.Status == "closed" && t.ClosedAt.HasValue).ToList();
            var avg = closed.Any()
                ? closed.Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalMinutes)
                : 0;

            result.Add(new BuildingPerformanceDto
            {
                BuildingId               = building.Id,
                BuildingName             = building.Name,
                OpenTickets              = openTickets,
                CriticalTickets          = criticalTickets,
                AvgResolutionTimeMinutes = Math.Round(avg, 1)
            });
        }

        return result;
    }
}
