using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using ShiftPlanner.Rules;

namespace ShiftPlanner.Engine;

internal static class EmployeeStandardRule
{
    public static void SpreadShiftsEvenlyThroughoutMonth(SchedulerParameters schedulingParams, ISchedulingEngine engine)
    {
        var daysInMonth = schedulingParams.Month.Days.Count;
        var length = daysInMonth / 4;
        var step = daysInMonth / 4;

        var ranges = new[]
        {
            (0 * step + 1, 0 * step + length),
            (1 * step + 1, 1 * step + length),
            (2 * step + 1, 2 * step + length),
            (3 * step + 1, daysInMonth)
        };

        foreach (var employeeMonth in schedulingParams.EmployeeMonths)
        {
            var workingDaysInMonth = schedulingParams.Month.Days
                .Where(d => !employeeMonth.Leaves.Any(l => l.Days.Contains(d)))
                .Count();

            foreach (var (from, to) in ranges)
            {
                var workingDaysInRange = schedulingParams.Month.Days
                    .Where(d => from <= d.Date.Day && d.Date.Day <= to)
                    .Where(d => !employeeMonth.Leaves.Any(l => l.Days.Contains(d)))
                    .Count();

                var standard = ComputeStandard(schedulingParams, employeeMonth, (double) workingDaysInRange / workingDaysInMonth, 0.1);

                var shiftsInRange = schedulingParams.Month.Days
                    .Where(d => from <= d.Date.Day && d.Date.Day <= to)
                    .SelectMany(d => schedulingParams.Workplaces.Select(w => new ShiftAssignment(employeeMonth.Employee, d, w)))
                    .ToArray();

                engine.AddSumConstraint(shiftsInRange, Constraint.Lte, standard.Maximum);
                // engine.AddSumConstraint(shiftsInRange, Constraint.Gte, standard.Minimum);
            }
        }
    }

    [Obsolete]
    private static EmployeeStandard ComputeStandard(
        SchedulerParameters monthScheduleParameters,
        EmployeeMonth employeeMonth,
        double multiplier,
        double flexibilityMultiplier = 1)
    {
        var daysCount = (double) monthScheduleParameters.Month.Days.Count;
        var totalRequiredCapacity = daysCount * monthScheduleParameters.Workplaces.Count;
        var capacityReductionByLeaves = employeeMonth.Leaves.Where(l => l.ReducesCapacity).Sum(l => l.LengthDays);
        var totalEmployeeCapacity = (double) monthScheduleParameters.EmployeeMonths.Sum(em => daysCount - em.Leaves.Where(l => l.ReducesCapacity).Sum(l => l.LengthDays));
        var employeeCapacity = daysCount - capacityReductionByLeaves;
        var flexibilityModifier = flexibilityMultiplier * monthScheduleParameters.Flexibility.Degree / 10;
        var preciseStandard = totalRequiredCapacity * employeeCapacity / totalEmployeeCapacity;
        var standardLb = Math.Max(employeeMonth.Employee.Workload.MinimumShiftsPerMonth * employeeCapacity / daysCount, multiplier * preciseStandard - flexibilityModifier);
        var standardUb = Math.Min(employeeMonth.Employee.Workload.MaximumShiftsPerMonth * employeeCapacity / daysCount, multiplier * preciseStandard + flexibilityModifier);

        return new EmployeeStandard(
            (uint) Math.Floor(standardLb),
            (uint) Math.Ceiling(standardUb));
    }
}
