using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

internal record SchedulerParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups,
    IReadOnlyCollection<ShiftAssignment> LastMonthShifts,
    IReadOnlyCollection<ShiftAssignment> FixedShifts,
    ScheduleFlexibility Flexibility
) : MonthScheduleParameters(Month, EmployeeMonths, CompetenceGroups, LastMonthShifts, FixedShifts);
