using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class EmployeeStandardRule
{
    public static void SpreadShiftAssignmentEvenly(SchedulingParameters schedulingParameters, ISchedulingEngine engine)
    {
        schedulingParameters.Employees.ToList().ForEach(e => AddEmployeeStandardRules(schedulingParameters, engine, e));
    }

    private static void AddEmployeeStandardRules(
        SchedulingParameters schedulingParameters,
        ISchedulingEngine engine,
        Employee employee)
    {
        var employeeStandard = ComputeStandard(schedulingParameters, employee);

        var employeeShiftsInMonth = schedulingParameters.Month.Days
            .SelectMany(d => schedulingParameters.Workplaces.Select(w => new ShiftAssignment(employee, d, w)))
            .ToList();

        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Gte, employeeStandard.Minimum);
        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Lte, employeeStandard.Maximum);
    }

    private static EmployeeStandard ComputeStandard(SchedulingParameters schedulingParameters, Employee employee)
    {
        decimal standard = (decimal) schedulingParameters.Month.Days.Count
                           * schedulingParameters.Workplaces.Count
                           / schedulingParameters.Employees.Count; 
        return new EmployeeStandard((int) Math.Floor(standard), (int) Math.Ceiling(standard));
    }

    private record struct EmployeeStandard(int Minimum, int Maximum);
}
