namespace AssetManagement.Models.DTOs.Dashboard;

public class DashboardStatsDto
{
    // Assets
    public int TotalAssets { get; set; }
    public int AssetsOperational { get; set; }
    public int AssetsUnderMaintenance { get; set; }

    // Tickets — current state
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int CriticalTickets { get; set; }
    public int OverdueTickets { get; set; }

    // Tickets — this month
    public int ResolvedThisMonth { get; set; }
    public int CreatedThisMonth { get; set; }
    public int OverdueThisMonth { get; set; }

    // Performance
    public double AvgResolutionTimeMinutes { get; set; }
    public double SLACompliancePercent { get; set; }   // % closed within SLA

    // Trend: tickets created per day for last 7 days (label + count)
    public List<TrendPointDto> Last7DaysTrend { get; set; } = new();

    // Breakdown
    public TicketsByPriorityDto TicketsByPriority { get; set; } = new();
    public TicketsByStatusDto TicketsByStatus { get; set; } = new();

    // Monthly resolution — last 6 months
    public List<MonthlyResolutionDto> MonthlyResolution { get; set; } = new();
}

public class TicketsByPriorityDto
{
    public int Critical { get; set; }
    public int Urgent { get; set; }
    public int Normal { get; set; }
    public int Low { get; set; }
}

public class TicketsByStatusDto
{
    public int Open { get; set; }
    public int InProgress { get; set; }
    public int Closed { get; set; }
}

public class TrendPointDto
{
    public string Label { get; set; } = string.Empty;  // e.g. "Mon", "Tue"
    public int Created { get; set; }
    public int Resolved { get; set; }
}

public class MonthlyResolutionDto
{
    public string Month { get; set; } = string.Empty;  // e.g. "Oct 25"
    public int Created { get; set; }
    public int Resolved { get; set; }
}
