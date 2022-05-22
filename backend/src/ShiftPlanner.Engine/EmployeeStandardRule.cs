using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class EmployeeStandardRule
{
    public static void SpreadShiftAssignmentEvenly(SchedulingParameters schedulingParameters, ISchedulingEngine engine)
    {
        schedulingParameters.EmployeeMonths.ToList().ForEach(em => AddEmployeeStandardRules(schedulingParameters, engine, em));
    }

    private static void AddEmployeeStandardRules(
        SchedulingParameters schedulingParameters,
        ISchedulingEngine engine,
        EmployeeMonth employeeMonth)
    {
        var employeeStandard = ComputeStandard(schedulingParameters, employeeMonth);

        var employeeShiftsInMonth = schedulingParameters.Month.Days
            .SelectMany(d => schedulingParameters.Workplaces.Select(w => new ShiftAssignment(employeeMonth.Employee, d, w)))
            .ToList();

        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Gte, employeeStandard.Minimum);
        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Lte, employeeStandard.Maximum);
    }

    private static EmployeeStandard ComputeStandard(SchedulingParameters schedulingParameters, EmployeeMonth employee)
    {
        var daysCount = (decimal) schedulingParameters.Month.Days.Count;
        var totalRequiredCapacity = daysCount * schedulingParameters.Workplaces.Count;
        var totalEmployeeCapacity =
            (decimal) schedulingParameters.EmployeeMonths.Sum(em => daysCount - em.Leaves.Sum(l => l.LengthDays));
        var employeeCapacity = daysCount - employee.Leaves.Sum(l => l.LengthDays);
        var standard = totalRequiredCapacity * employeeCapacity / totalEmployeeCapacity;
        
        return new EmployeeStandard((int) Math.Floor(standard), (int) Math.Ceiling(standard));
    }

    private record struct EmployeeStandard(int Minimum, int Maximum);
}
