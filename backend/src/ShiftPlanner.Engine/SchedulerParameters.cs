using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

internal record SchedulerParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups,
    IReadOnlyCollection<ShiftAssignment> LastMonthShifts,
    IReadOnlyCollection<ShiftAssignment> FixedShifts,
    IReadOnlyCollection<ISet<Workplace>> WeekendInterchangeabilityGroups,
    ScheduleFlexibility Flexibility
) : MonthScheduleParameters(Month, EmployeeMonths, CompetenceGroups, LastMonthShifts, FixedShifts, WeekendInterchangeabilityGroups);
