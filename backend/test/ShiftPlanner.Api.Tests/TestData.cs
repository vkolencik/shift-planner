using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests;

internal static class TestData
{
    public static Dictionary<Day, string> Requirements = new Dictionary<Day, string>
    {
        [new Day(2022, 10, 12)] = "hard-no"
    };

    public static readonly Tenant Tenant = new Tenant("tenant-1");
    public static readonly ScheduledMonth Month = new ScheduledMonth(2020, 10);
}
