namespace AssetManagement.Core.Helpers;

public static class SLACalculator
{
    public static int GetSLAHours(string priority) => priority.ToLower() switch
    {
        "critical" => 4,
        "urgent"   => 12,
        "normal"   => 24,
        "low"      => 48,
        _          => 24
    };

    public static DateTime CalculateDueDate(DateTime createdAt, int slaHours)
        => createdAt.AddHours(slaHours);
}
