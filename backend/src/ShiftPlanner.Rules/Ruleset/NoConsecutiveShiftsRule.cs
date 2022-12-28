using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class NoConsecutiveShiftsRule : IRule
{
    private readonly MonthScheduleParameters _parameters;

    public NoConsecutiveShiftsRule(MonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var employee in _parameters.Employees)
        {
            foreach (var (day1, day2) in _parameters.Days.Zip(_parameters.Days.Skip(1)))
            {
                var employeesShiftsInTwoDays = _parameters.Workplaces
                    .SelectMany(w => new[] {new ShiftAssignment(employee, day1, w), new ShiftAssignment(employee, day2, w)})
                    .ToArray();

                target.AddSumConstraint(employeesShiftsInTwoDays, Constraint.Lte, 1);
            }
        }

        var firstDayOfMonth = _parameters.Days.First();
        var lastDayOfPreviousMonth = _parameters.Month.Previous.Days.Last();
        var shiftsOnTheLastDayOfPreviousMonth = _parameters.LastMonthShifts.Where(s => s.Shift.Day == lastDayOfPreviousMonth);
        foreach (var employee in new HashSet<Employee>(shiftsOnTheLastDayOfPreviousMonth.Select(s => s.Employee)))
        {
            var shiftsOnFirstDay = _parameters.Workplaces
                .Select(w => new ShiftAssignment(employee, firstDayOfMonth, w))
                .ToArray();

            target.AddSumConstraint(shiftsOnFirstDay, Constraint.Eq, 0);
        }
    }
}
