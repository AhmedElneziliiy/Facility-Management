using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Core.Helpers;

public static class TicketNumberGenerator
{
    public static async Task<string> GenerateAsync(ApplicationDbContext context)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"TKT-{year}-";

        var lastTicket = await context.Tickets
            .Where(t => t.TicketNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TicketNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastTicket != null)
        {
            var numberPart = lastTicket.TicketNumber.Substring(prefix.Length);
            if (int.TryParse(numberPart, out var parsed))
                nextNumber = parsed + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
