using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class NoAlternativeDayShiftsRule : IRule
{
    private readonly MonthScheduleParameters _parameters;

    public NoAlternativeDayShiftsRule(MonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        foreach (var employee in _parameters.Employees)
        {
            foreach (var (day1, day3) in _parameters.Days.Zip(_parameters.Days.Skip(2)))
            {
                if (day1.Date.DayOfWeek == DayOfWeek.Friday)
                {
                    continue;
                }

                var employeesShiftsInAlternateDays = _parameters.Workplaces
                    .SelectMany(w => new[] {new ShiftAssignment(employee, day1, w), new ShiftAssignment(employee, day3, w)})
                    .ToArray();

                target.AddSumConstraint(employeesShiftsInAlternateDays, Constraint.Lte, 1);
            }
        }

        var previousMonthDays = _parameters.Month.Previous.Days;

        var lastDayOfPreviousMonth = previousMonthDays.Last();
        ForbidShiftsOnDayWhenLastMonthDay(lastDayOfPreviousMonth);

        var penultimateDayOfPreviousMonth = previousMonthDays.ElementAt(previousMonthDays.Count - 2);
        ForbidShiftsOnDayWhenLastMonthDay(penultimateDayOfPreviousMonth);

        void ForbidShiftsOnDayWhenLastMonthDay(Day previousMonthDay)
        {
            if (previousMonthDay.DayOfWeek == DayOfWeek.Friday)
            {
                return;
            }

            var thisMonthDay = new Day(previousMonthDay.Date.AddDays(2));
            var shiftsOnPreviousMonthDay = _parameters.LastMonthShifts.Where(s => s.Shift.Day == previousMonthDay);
            foreach (var employee in new HashSet<Employee>(shiftsOnPreviousMonthDay.Select(s => s.Employee)))
            {
                var shiftsOnDay = _parameters.Workplaces
                    .Select(w => new ShiftAssignment(employee, thisMonthDay, w))
                    .ToArray();

                target.AddSumConstraint(shiftsOnDay, Constraint.Eq, 0);
            }
        }
    }
}
