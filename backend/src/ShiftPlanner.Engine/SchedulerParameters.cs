using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

internal record SchedulerParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups,
    ScheduleFlexibility Flexibility
) : MonthScheduleParameters(Month, EmployeeMonths, CompetenceGroups);
