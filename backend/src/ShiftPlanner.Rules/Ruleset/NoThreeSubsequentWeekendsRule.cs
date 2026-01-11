using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class NoThreeSubsequentWeekendsRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public NoThreeSubsequentWeekendsRule(IMonthScheduleParameters parameters)
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
                var nextNextSaturday = new Day(saturday.Date.AddDays(14));
                var nextNextSunday = new Day(sunday.Date.AddDays(14));
                if (nextNextSaturday.Month != _parameters.Month.Month) continue;

                var shiftsOverSubsequentWeekends = new[] {saturday, sunday, nextSaturday, nextSunday,nextNextSaturday, nextNextSunday }
                    .Where(d => d.Month == _parameters.Month.Month)
                    .SelectMany(d => _parameters.Workplaces.Select(w => new ShiftAssignment(employee, d, w)))
                    .ToArray();

                target.AddSumConstraint(shiftsOverSubsequentWeekends, Constraint.Lte, 2);
            }
        }
    }
}
