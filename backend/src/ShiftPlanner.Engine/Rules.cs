using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using ShiftPlanner.Rules;

namespace ShiftPlanner.Engine;

internal static class Rules
{
    public static void DisallowShiftsWithoutEmployeeCapability(ISolverParameters solverParameters, ISchedulingEngine engine)
    {
        foreach (var employee in solverParameters.Employees)
        {
            var incapableWorkplaces = solverParameters.Workplaces.Except(employee.Capabilities.AllowedWorkplaces);

            foreach (var incapableWorkplace in incapableWorkplaces)
            {
                foreach (var day in solverParameters.Days)
                {
                    engine.AddVariableConstraint(new ShiftAssignment(employee, day, incapableWorkplace), Constraint.Eq, 0);
                }
            }
        }
    }

    public static void TakeLeavesIntoAccount(MonthScheduleParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        foreach (var employeeMonth in monthScheduleParameters.EmployeeMonths)
        {
            foreach (var leave in employeeMonth.Leaves)
            {
                foreach (var day in leave.Days)
                {
                    foreach (var workplace in monthScheduleParameters.Workplaces)
                    {
                        engine.AddVariableConstraint(new ShiftAssignment(employeeMonth.Employee, day, workplace), Constraint.Eq, 0);
                    }
                }
            }
        }
    }

    public static void EnforceFixedShifts(MonthScheduleParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        foreach (var fixedShift in monthScheduleParameters.FixedShifts)
        {
            engine.AddVariableConstraint(fixedShift, Constraint.Eq, 1);
        }
    }
}
