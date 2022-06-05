using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

public class ShiftScheduler
{
    public Schedule ComputeSchedule(MonthScheduleParameters monthScheduleParameters)
    {
        var solverParameters = GetSolverParameters(monthScheduleParameters);
        for (uint flexibility = 0; flexibility <= 5; flexibility++)
        {
            var schedulingParameters2 = GetSchedulingParameters2(monthScheduleParameters, new ScheduleFlexibility(flexibility));
            var engine = SchedulingEngine.Engine.CreateGoogleSolverEngine(solverParameters);

            Rules.EveryDayAtEveryWorkplaceIsCovered(solverParameters, engine);

            Rules.EveryEmployeeHasOneShiftAtATime(solverParameters, engine);

            Rules.NoConsecutiveShiftsForEmployee(solverParameters, engine);

            Rules.NoAlternativeDayShiftsForEmployee(solverParameters, engine);

            Rules.LimitWeekendShifts(monthScheduleParameters, engine);

            Rules.EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(
                monthScheduleParameters, solverParameters.Days, engine);

            Rules.DisallowShiftsWithoutEmployeeCapability(solverParameters, engine);

            EmployeeStandardRule.SpreadShiftAssignmentEvenly(schedulingParameters2, engine);

            EmployeeStandardRule.AssignShiftsByPreferencePercentages(schedulingParameters2, engine);

            Rules.TakeLeavesIntoAccount(monthScheduleParameters, engine);

            var result = engine.Solve();

            if (result is Schedule schedule)
            {
                return schedule;
            }
        }

        throw new Exception("Cannot find feasible solution");
    }

    private SchedulerParameters GetSchedulingParameters2(MonthScheduleParameters monthScheduleParameters, ScheduleFlexibility flexiblity)
    {
        return new SchedulerParameters(
            monthScheduleParameters.Month,
            monthScheduleParameters.EmployeeMonths, 
            monthScheduleParameters.CompetenceGroups, 
            flexiblity);
    }

    private static SolverParameters GetSolverParameters(MonthScheduleParameters monthScheduleParameters)
    {
        var days = monthScheduleParameters.Month.Days;

        return new SolverParameters(
            days,
            monthScheduleParameters.Employees,
            monthScheduleParameters.CompetenceGroups.SelectMany(g => g.Workplaces).ToArray());
    }
}
