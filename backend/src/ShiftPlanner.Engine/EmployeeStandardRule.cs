using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class EmployeeStandardRule
{
    public static void SpreadShiftAssignmentEvenly(SchedulingParameters schedulingParameters, ISchedulingEngine engine)
    {
        schedulingParameters.EmployeeMonths.ToList().ForEach(em => AddEmployeeStandardRules(schedulingParameters, engine, em));
    }

    public static void AssignShiftsByPreferencePercentages(SchedulingParameters schedulingParameters,
        ISchedulingEngine engine)
    {
        schedulingParameters.EmployeeMonths.ToList().ForEach(em => AddEmployeePreferencePercentages(schedulingParameters, engine, em));

    }

    private static void AddEmployeeStandardRules(
        SchedulingParameters schedulingParameters,
        ISchedulingEngine engine,
        EmployeeMonth employeeMonth)
    {
        var employeeStandard = ComputeStandard(schedulingParameters, employeeMonth, 1);

        var employeeShiftsInMonth = schedulingParameters.Month.Days
            .SelectMany(d => schedulingParameters.Workplaces.Select(w => new ShiftAssignment(employeeMonth.Employee, d, w)))
            .ToList();

        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Gte, employeeStandard.Minimum);
        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Lte, employeeStandard.Maximum);
    }

    private static void AddEmployeePreferencePercentages(
        SchedulingParameters schedulingParameters,
        ISchedulingEngine engine,
        EmployeeMonth employeeMonth)
    {
        foreach (var capability in employeeMonth.Employee.Capabilities.WorkplaceCapabilities
                     .OfType<WorkplaceCapability.Green>())
        {
            var employeeStandardForWorkplace = ComputeStandard(schedulingParameters, employeeMonth, (double) capability.PreferencePercentage / 100);

            var employeeShiftsInMonthForWorkspace = schedulingParameters.Month.Days
                .Select(d => new ShiftAssignment(employeeMonth.Employee, d, capability.Workplace))
                .ToList();

            engine.AddSumConstraint(employeeShiftsInMonthForWorkspace, Constraint.Gte, employeeStandardForWorkplace.Minimum);
            engine.AddSumConstraint(employeeShiftsInMonthForWorkspace, Constraint.Lte, employeeStandardForWorkplace.Maximum);
        }
    }

    private static EmployeeStandard ComputeStandard(SchedulingParameters schedulingParameters, EmployeeMonth employee, double multiplier)
    {
        var daysCount = (double) schedulingParameters.Month.Days.Count;
        var totalRequiredCapacity = daysCount * schedulingParameters.Workplaces.Count;
        var totalEmployeeCapacity =
            (double) schedulingParameters.EmployeeMonths.Sum(em => daysCount - em.Leaves.Sum(l => l.LengthDays));
        var employeeCapacity = daysCount - employee.Leaves.Sum(l => l.LengthDays);
        var standard = multiplier * totalRequiredCapacity * employeeCapacity / totalEmployeeCapacity;
        
        return new EmployeeStandard((int) Math.Floor(standard), (int) Math.Ceiling(standard));
    }

    private record struct EmployeeStandard(int Minimum, int Maximum);
}
