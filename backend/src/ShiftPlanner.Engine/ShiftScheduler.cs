using ShiftPlanner.Domain;
using ShiftPlanner.Engine.GoogleCp;

namespace ShiftPlanner.Engine;

public class ShiftScheduler
{
    public Schedule ComputeSchedule(SchedulingParameters schedulingParameters)
    {
        var solverParameters = GetSolverParameters(schedulingParameters);
        var engine = new SchedulingEngine(new GoogleCpSolver(), solverParameters);
        
        EveryShiftIsCovered(solverParameters, engine);
        
        EveryEmployeeHasOneShiftAtATime(solverParameters, engine);

        NoConsecutiveShiftsForEmployee(solverParameters, engine);

        return engine.Solve();
    }

    private static SolverParameters GetSolverParameters(SchedulingParameters schedulingParameters)
    {
        var days = Day.CreateDaysForMonth(schedulingParameters.Month);

        return new SolverParameters(
            days,
            schedulingParameters.Employees,
            schedulingParameters.Workplaces);
    }

    private static void EveryShiftIsCovered(SolverParameters schedulingParameters, SchedulingEngine engine)
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

    private static void EveryEmployeeHasOneShiftAtATime(SolverParameters schedulingParameters, SchedulingEngine engine)
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

    private static void NoConsecutiveShiftsForEmployee(SolverParameters schedulingParameters, SchedulingEngine engine)
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
