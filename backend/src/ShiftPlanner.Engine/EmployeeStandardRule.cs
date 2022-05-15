using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class EmployeeStandardRule
{
    public static void SpreadShiftAssignmentEvenly(SolverParameters solverParameters, ISchedulingEngine engine)
    {
        solverParameters.Employees.ToList().ForEach(e => AddEmployeeStandardRules(solverParameters, engine, e));
    }

    private static void AddEmployeeStandardRules(
        SolverParameters solverParameters,
        ISchedulingEngine engine,
        Employee employee)
    {
        var employeeStandard = ComputeStandard(solverParameters, employee);

        var employeeShiftsInMonth = solverParameters.Days
            .SelectMany(d => solverParameters.Workplaces.Select(w => new ShiftAssignment(employee, d, w)))
            .ToList();

        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Gte, employeeStandard.Minimum);
        engine.AddSumConstraint(employeeShiftsInMonth, Constraint.Lte, employeeStandard.Maximum);
    }

    private static EmployeeStandard ComputeStandard(SolverParameters solverParameters, Employee employee)
    {
        decimal standard = (decimal) solverParameters.Days.Count
                           * solverParameters.Workplaces.Count
                           / solverParameters.Employees.Count; 
        return new EmployeeStandard((int) Math.Floor(standard), (int) Math.Ceiling(standard));
    }

    private record struct EmployeeStandard(int Minimum, int Maximum);
}
