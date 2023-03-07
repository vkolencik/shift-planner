using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class WeekendShiftLimitRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public WeekendShiftLimitRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        LimitShiftsOnDays(_parameters, target, _parameters.Days.Where(d => d.DayOfWeek is DayOfWeek.Saturday), 1);
        LimitShiftsOnDays(_parameters, target, _parameters.Days.Where(d => d.DayOfWeek is DayOfWeek.Sunday), 2);
    }

    private static void LimitShiftsOnDays(
        IMonthScheduleParameters monthScheduleParameters,
        IScheduleConstraintReceiver target,
        IEnumerable<Day> days,
        uint limit)
    {
        foreach (var employeeMonth in monthScheduleParameters.EmployeeMonths)
        {
            var shiftsOnTheWeekday = monthScheduleParameters.Workplaces
                .SelectMany(w => days.Select(d => new ShiftAssignment(employeeMonth.Employee, d, w)))
                .ToArray();

            target.AddSumConstraint(shiftsOnTheWeekday, Constraint.Lte, limit);
        }
    }
}
