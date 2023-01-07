using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class MinNumberOfWeekendShiftsRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public MinNumberOfWeekendShiftsRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        var weekendDays = _parameters.Month.Days.Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday).ToHashSet();
        
        foreach (var employeeMonth in _parameters.EmployeeMonths)
        {
            if (!weekendDays.Except(employeeMonth.Leaves.SelectMany(l => l.Days).ToHashSet()).Any())
            {
                continue;
            }

            var weekendShifts = weekendDays
                .SelectMany(d => _parameters.Workplaces
                    .Select(w => new ShiftAssignment(employeeMonth.Employee, d, w)))
                .ToArray();
            target.AddSumConstraint(weekendShifts, Constraint.Gte, 1);
        }
    }
}
