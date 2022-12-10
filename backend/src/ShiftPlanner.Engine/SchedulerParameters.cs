using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

internal record SchedulerParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups,
    Schedule LastMonthSchedule,
    ScheduleFlexibility Flexibility
) : MonthScheduleParameters(Month, EmployeeMonths, CompetenceGroups, LastMonthSchedule);
