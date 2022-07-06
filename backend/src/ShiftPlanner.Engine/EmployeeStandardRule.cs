using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class EmployeeStandardRule
{
    public static void SpreadShiftAssignmentEvenlyAmongEmployees(SchedulerParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        monthScheduleParameters.EmployeeMonths.ToList().ForEach(em => AddEmployeeStandardRules(monthScheduleParameters, engine, em));
    }

    public static void AssignShiftsByPreferencePercentages(
        SchedulerParameters monthScheduleParameters,
        ISchedulingEngine engine)
    {
        monthScheduleParameters.EmployeeMonths.ToList().ForEach(em => AddEmployeePreferencePercentages(monthScheduleParameters, engine, em));
    }

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
                engine.AddSumConstraint(shiftsInRange, Constraint.Gte, standard.Minimum);
            }
        }
    }

    private static void AddEmployeeStandardRules(SchedulerParameters monthScheduleParameters,
        ISchedulingEngine engine,
        EmployeeMonth employeeMonth)
    {
        var employeeStandard = ComputeStandard(monthScheduleParameters, employeeMonth, 1);

        var employeeShiftsInMonth = monthScheduleParameters.Month.Days
            .SelectMany(d => monthScheduleParameters.Workplaces.Select(w => new ShiftAssignment(employeeMonth.Employee, d, w)))
            .ToList();

        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Gte, employeeStandard.Minimum);
        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Lte, employeeStandard.Maximum);
    }

    private static void AddEmployeePreferencePercentages(SchedulerParameters monthScheduleParameters,
        ISchedulingEngine engine,
        EmployeeMonth employeeMonth)
    {
        foreach (var capability in employeeMonth.Employee.Capabilities.WorkplaceCapabilities
                     .OfType<WorkplaceCapability.Green>())
        {
            var employeeStandardForWorkplace = ComputeStandard(monthScheduleParameters, employeeMonth, (double) capability.PreferencePercentage / 100);

            var employeeShiftsInMonthForWorkspace = monthScheduleParameters.Month.Days
                .Select(d => new ShiftAssignment(employeeMonth.Employee, d, capability.Workplace))
                .ToList();

            engine.AddSumConstraint(employeeShiftsInMonthForWorkspace, Constraint.Gte, employeeStandardForWorkplace.Minimum);
            engine.AddSumConstraint(employeeShiftsInMonthForWorkspace, Constraint.Lte, employeeStandardForWorkplace.Maximum);
        }
    }

    private static EmployeeStandard ComputeStandard(
        SchedulerParameters monthScheduleParameters,
        EmployeeMonth employeeMonth,
        double multiplier,
        double flexibilityMultiplier = 1)
    {
        var daysCount = (double) monthScheduleParameters.Month.Days.Count;
        var totalRequiredCapacity = daysCount * monthScheduleParameters.Workplaces.Count;
        var totalEmployeeCapacity = (double) monthScheduleParameters.EmployeeMonths.Sum(em => daysCount - em.Leaves.Sum(l => l.LengthDays));
        var employeeCapacity = daysCount - employeeMonth.Leaves.Sum(l => l.LengthDays);
        var flexibilityModifier = flexibilityMultiplier * monthScheduleParameters.Flexibility.Degree / 10;
        var preciseStandard = totalRequiredCapacity * employeeCapacity / totalEmployeeCapacity;
        var standardLb = Math.Max(0, multiplier * preciseStandard - flexibilityModifier);
        var standardUb = Math.Min(employeeMonth.Employee.MaxShiftsPerMonth, multiplier * preciseStandard + flexibilityModifier);

        return new EmployeeStandard(
            (uint) Math.Floor(standardLb),
            (uint) Math.Ceiling(standardUb));
    }

    private record struct EmployeeStandard(uint Minimum, uint Maximum);
}
