using System.Diagnostics.CodeAnalysis;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class EvenSpreadWithinMonthRule : IRule
{
    private readonly IEmployeeStandardCalculator _standardCalculator;
    private readonly IMonthScheduleParameters _parameters;

    public EvenSpreadWithinMonthRule(IEmployeeStandardCalculator standardCalculator, IMonthScheduleParameters parameters)
    {
        _standardCalculator = standardCalculator;
        _parameters = parameters;
    }

    [SuppressMessage("ReSharper", "UselessBinaryOperation")]
    public void Apply(IScheduleConstraintReceiver target)
    {
        var daysInMonth = _parameters.Month.Days.Count;
        var length = daysInMonth / 4;
        var step = daysInMonth / 4;

        var ranges = new[]
        {
            (0 * step + 1, 0 * step + length),
            (1 * step + 1, 1 * step + length),
            (2 * step + 1, 2 * step + length),
            (3 * step + 1, daysInMonth)
        };

        foreach (var employeeMonth in _parameters.EmployeeMonths)
        {
            var workingDaysInMonth = _parameters.Month.Days
                .Where(d => !employeeMonth.Leaves.Any(l => l.Days.Contains(d)))
                .Count();

            foreach (var (from, to) in ranges)
            {
                var workingDaysInRange = _parameters.Month.Days
                    .Where(d => from <= d.Date.Day && d.Date.Day <= to)
                    .Where(d => !employeeMonth.Leaves.Any(l => l.Days.Contains(d)))
                    .Count();

                var standard = _standardCalculator.ComputeStandard(employeeMonth, (double) workingDaysInRange / workingDaysInMonth, 0.1);

                var shiftsInRange = _parameters.Month.Days
                    .Where(d => from <= d.Date.Day && d.Date.Day <= to)
                    .SelectMany(d => _parameters.Workplaces.Select(w => new ShiftAssignment(employeeMonth.Employee, d, w)))
                    .ToArray();

                target.AddSumConstraint(shiftsInRange, Constraint.Lte, standard.Maximum);
                // target.AddSumConstraint(shiftsInRange, Constraint.Gte, standard.Minimum);
            }
        }

    }
}
