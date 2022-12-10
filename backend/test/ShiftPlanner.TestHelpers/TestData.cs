using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.TestHelpers;

public static class TestData
{
    public static readonly Dictionary<Day, string> MonthRequirements = new Dictionary<Day, string>
    {
        [new Day(2022, 10, 12)] = "hard-no"
    };

    public static readonly Tenant Tenant = new Tenant("tenant-1");
    public static readonly ScheduledMonth Month = new ScheduledMonth(2020, 10);
    public static readonly Day Day = new Day(Month, 13);
    public static readonly RequirementModel Requirement = new RequirementModel(Tenant, "user-1", Day, "hard-no");
    public static readonly Workplace Workplace = new Workplace("workplace1");
}
