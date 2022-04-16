using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine.GoogleCp;

namespace ShiftPlanner.Engine;

public class ShiftScheduler
{
    public Schedule ComputeSchedule(SchedulingParameters schedulingParameters)
    {
        var solverParameters = GetSolverParameters(schedulingParameters);
        var engine = new SchedulingEngine.SchedulingEngine(new GoogleCpSolver(), solverParameters);

        Rules.EveryDayAtEveryWorkplaceIsCovered(solverParameters, engine);

        Rules.EveryEmployeeHasOneShiftAtATime(solverParameters, engine);

        Rules.NoConsecutiveShiftsForEmployee(solverParameters, engine);

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
}
