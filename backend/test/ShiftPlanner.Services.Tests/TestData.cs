using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Tests;

public static class TestData
{
    public static readonly Dictionary<Day, string> MonthRequirements = new Dictionary<Day, string>
    {
        [new Day(2022, 10, 12)] = "hard-no"
    };

    public static readonly Tenant Tenant = new Tenant("tenant-1");
    public static readonly ScheduledMonth Month = new ScheduledMonth(2020, 10);
    public static readonly Day Day = new Day(Month, 13);
    public static readonly Requirement Requirement = new Requirement(Tenant, "user-1", Day, "hard-no");
    public static readonly Workplace Workplace = new Workplace("workplace1");

    public static MonthScheduleParameters MonthSchedulingParameters => new MonthScheduleParameters(
        Month, Array.Empty<Employee>(), Array.Empty<EmployeeMonth>(), Array.Empty<CompetenceGroup>(), Array.Empty<ShiftAssignment>(), Array.Empty<ShiftAssignment>(), Array.Empty<ISet<Workplace>>());
}
