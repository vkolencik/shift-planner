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
        foreach (var employeeMonth in _parameters.EmployeeMonths)
        {
            var maxSats = 2u;
            var maxSuns = 2u;
            var maxWeekends = 3u;

            if (employeeMonth.Employee.Name == "Jánský")
            {
                maxSats = 0;
                maxSuns = 1;
                maxWeekends = 1u;
            }
            
            LimitShiftsOnDays(employeeMonth.Employee, _parameters.Workplaces, target, _parameters.Days.Where(d => d.DayOfWeek is DayOfWeek.Saturday), maxSats);
            LimitShiftsOnDays(employeeMonth.Employee, _parameters.Workplaces, target, _parameters.Days.Where(d => d.DayOfWeek is DayOfWeek.Sunday), maxSuns);
            LimitShiftsOnDays(employeeMonth.Employee, _parameters.Workplaces, target, _parameters.Days.Where(d => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday), maxWeekends);
        }
    }

    private static void LimitShiftsOnDays(
        Employee employee,
        IReadOnlyCollection<Workplace> workplaces,
        IScheduleConstraintReceiver target,
        IEnumerable<Day> days,
        uint limit)
    {
        var shiftsOnTheWeekday = workplaces
            .SelectMany(w => days.Select(d => new ShiftAssignment(employee, d, w)))
            .ToArray();

        target.AddSumConstraint(shiftsOnTheWeekday, Constraint.Lte, limit);
    }
}
