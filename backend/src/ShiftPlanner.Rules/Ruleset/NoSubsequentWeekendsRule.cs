using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class NoSubsequentWeekendsRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public NoSubsequentWeekendsRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        var weekendDays = _parameters.Days.Where(d => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday).ToArray();

        var saturdays = weekendDays
            .Concat(weekendDays.Where(d => d.DayOfWeek is DayOfWeek.Saturday).Select(d => new Day(d.Date.AddDays(1))))
            .Concat(weekendDays.Where(d => d.DayOfWeek is DayOfWeek.Sunday).Select(d => new Day(d.Date.AddDays(-1))))
            .Where(d => d.DayOfWeek is DayOfWeek.Saturday)
            .ToHashSet()
            .OrderBy(_ => _)
            .ToList(); // might also include last saturday of last month

        foreach (var employee in _parameters.Employees)
        {
            foreach (var saturday in saturdays)
            {
                var sunday = new Day(saturday.Date.AddDays(1));
                var nextSaturday = new Day(saturday.Date.AddDays(7));
                var nextSunday = new Day(sunday.Date.AddDays(7));
                if (nextSaturday.Month != _parameters.Month.Month) continue;

                var shiftsOverSubsequentWeekends = new[] {saturday, sunday, nextSaturday, nextSunday}
                    .Where(d => d.Month == _parameters.Month.Month)
                    .SelectMany(d => _parameters.Workplaces.Select(w => new ShiftAssignment(employee, d, w)))
                    .ToArray();

                target.AddSumConstraint(shiftsOverSubsequentWeekends, Constraint.Lte, 1);
            }
        }

        void LimitShiftsOnWeekend(Day saturdayOfLastMonth)
        {
            var lastWeekendOfLastMonth = new[]
            {
                saturdayOfLastMonth,
                new Day(saturdayOfLastMonth.Date.AddDays(1))
            }.Where(d => d.Month == _parameters.Month.Previous.Month);

            var subsequentWeekendSaturday = new Day(saturdayOfLastMonth.Date.AddDays(7));
            var subsequentWeekend = new[] {subsequentWeekendSaturday, new Day(subsequentWeekendSaturday.Date.AddDays(1))}
                .Where(d => d.Month == _parameters.Month.Month)
                .ToArray();

            if (!subsequentWeekend.Any())
            {
                return;
            }

            var lastMonthWeekendShifts = _parameters.ActiveEmployeeLastMonthShifts.Where(s => lastWeekendOfLastMonth.Contains(s.Shift.Day));
            foreach (var employee in lastMonthWeekendShifts.Select(s => s.Employee).ToHashSet())
            {
                target.AddSumConstraint(_parameters.Workplaces.SelectMany(w => subsequentWeekend.Select(d => new ShiftAssignment(employee, d, w))).ToArray(), Constraint.Eq, 0);
            }
        }

        var lastSaturdays = _parameters.Month.Previous.Days.Reverse().Where(d => d.DayOfWeek is DayOfWeek.Saturday).ToArray();

        var lastSaturdayOfLastMonth = lastSaturdays.ElementAt(0);
        LimitShiftsOnWeekend(lastSaturdayOfLastMonth);

        var penultimateSaturdayOfLastMonth = lastSaturdays.ElementAt(1);
        LimitShiftsOnWeekend(penultimateSaturdayOfLastMonth);
    }
}
