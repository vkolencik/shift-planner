using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class EmployeeStandardRule
{
    public static void SpreadShiftAssignmentEvenly(SchedulerParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        monthScheduleParameters.EmployeeMonths.ToList().ForEach(em => AddEmployeeStandardRules(monthScheduleParameters, engine, em));
    }

    public static void AssignShiftsByPreferencePercentages(
        SchedulerParameters monthScheduleParameters,
        ISchedulingEngine engine)
    {
        monthScheduleParameters.EmployeeMonths.ToList().ForEach(em => AddEmployeePreferencePercentages(monthScheduleParameters, engine, em));

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

    private static EmployeeStandard ComputeStandard(SchedulerParameters monthScheduleParameters, EmployeeMonth employeeMonth, double multiplier)
    {
        var daysCount = (double) monthScheduleParameters.Month.Days.Count;
        var totalRequiredCapacity = daysCount * monthScheduleParameters.Workplaces.Count;
        var totalEmployeeCapacity = (double) monthScheduleParameters.EmployeeMonths.Sum(em => daysCount - em.Leaves.Sum(l => l.LengthDays));
        var employeeCapacity = daysCount - employeeMonth.Leaves.Sum(l => l.LengthDays);
        var flexibilityModifier = (double) monthScheduleParameters.Flexibility.Degree / 10;
        var standardLb = Math.Max(0, multiplier * (totalRequiredCapacity * employeeCapacity / totalEmployeeCapacity) - flexibilityModifier);
        var standardUb = Math.Min(employeeMonth.Employee.MaxShiftsPerMonth, multiplier * (totalRequiredCapacity * employeeCapacity / totalEmployeeCapacity) + flexibilityModifier);

        return new EmployeeStandard(
            (uint) Math.Floor(standardLb),
            (uint) Math.Ceiling(standardUb));
    }

    private record struct EmployeeStandard(uint Minimum, uint Maximum);
}
