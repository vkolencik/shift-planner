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
        
        Rules.NoAlternativeDayShiftsForEmployee(solverParameters, engine);
        
        Rules.EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(schedulingParameters, solverParameters.Days, engine);
        
        Rules.DisallowShiftsWithoutEmployeeCapability(solverParameters, engine);
        
        EmployeeStandardRule.SpreadShiftAssignmentEvenly(schedulingParameters, engine);

        Rules.TakeLeavesIntoAccount(schedulingParameters, engine);

        return engine.Solve();
    }

    private static SolverParameters GetSolverParameters(SchedulingParameters schedulingParameters)
    {
        var days = schedulingParameters.Month.Days;

        return new SolverParameters(
            days,
            schedulingParameters.Employees,
            schedulingParameters.CompetenceGroups.SelectMany(g => g.Workplaces).ToArray());
    }
}
