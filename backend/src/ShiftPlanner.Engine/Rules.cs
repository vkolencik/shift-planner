using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class Rules
{
    public static void EveryDayAtEveryWorkplaceIsCovered(SolverParameters schedulingParameters, ISchedulingEngine engine)
    {
        foreach (var day in schedulingParameters.Days)
        {
            foreach (var workplace in schedulingParameters.Workplaces)
            {
                var shiftsOnDayAtWorkplace = schedulingParameters.Employees
                    .Select(e => new Shift(e, day, workplace))
                    .ToArray();

                engine.AddSumConstraint(shiftsOnDayAtWorkplace, Constraint.Eq, 1);
            }
        }
    }

    public static void EveryEmployeeHasOneShiftAtATime(SolverParameters schedulingParameters, ISchedulingEngine engine)
    {
        foreach (var employee in schedulingParameters.Employees)
        {
            foreach (var day in schedulingParameters.Days)
            {
                var employeesShiftsThatDay = schedulingParameters.Workplaces
                    .Select(w => new Shift(employee, day, w))
                    .ToArray();
 
                engine.AddSumConstraint(employeesShiftsThatDay, Constraint.Lt, 1);
            }
        }
    }

    public static void NoConsecutiveShiftsForEmployee(SolverParameters schedulingParameters, ISchedulingEngine engine)
    {
        foreach (var employee in schedulingParameters.Employees)
        {
            foreach (var (day1, day2) in schedulingParameters.Days.Zip(schedulingParameters.Days.Skip(1)))
            {
                var employeesShiftsInTwoDays = schedulingParameters.Workplaces
                    .SelectMany(w => new [] {new Shift(employee, day1, w), new Shift(employee, day2, w)})
                    .ToArray();
 
                engine.AddSumConstraint(employeesShiftsInTwoDays, Constraint.Lt, 1);
            }
        }
    }
}
