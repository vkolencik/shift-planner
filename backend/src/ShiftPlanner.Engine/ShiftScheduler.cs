using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

public class ShiftScheduler
{
    public SchedulingResult ComputeSchedule(MonthScheduleParameters monthScheduleParameters)
    {
        var solverParameters = GetSolverParameters(monthScheduleParameters);
        for (uint degreeOfFlexibility = 0; degreeOfFlexibility <= 50; degreeOfFlexibility++)
        {
            var flexibility = new ScheduleFlexibility(degreeOfFlexibility);
            var schedulerParameters = GetSchedulerParameters(monthScheduleParameters, flexibility);
            var engine = SchedulingEngine.Engine.CreateGoogleSolverEngine(solverParameters);

            Rules.EveryDayAtEveryWorkplaceIsCovered(solverParameters, engine);

            Rules.EveryEmployeeHasOneShiftAtATime(solverParameters, engine);

            Rules.NoConsecutiveShiftsForEmployee(monthScheduleParameters, engine);

            Rules.NoAlternativeDayShiftsForEmployee(monthScheduleParameters, engine);

            Rules.LimitWeekendShifts(monthScheduleParameters, engine);

            Rules.AlwaysScheduleAlternatesOverWeekends(monthScheduleParameters, engine);

            Rules.EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(
                monthScheduleParameters, solverParameters.Days, engine);

            Rules.DisallowShiftsWithoutEmployeeCapability(solverParameters, engine);

            EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulerParameters, engine);

            EmployeeStandardRule.AssignShiftsByPreferencePercentages(schedulerParameters, engine);

            EmployeeStandardRule.SpreadShiftsEvenlyThroughoutMonth(schedulerParameters, engine);

            Rules.TakeLeavesIntoAccount(monthScheduleParameters, engine);
            
            Rules.EnforceFixedShifts(monthScheduleParameters, engine);

            var result = engine.Solve();

            if (result != null)
            {
                return new SuccessfulSchedulingResult(result, flexibility);
            }
        }

        throw new Exception("Cannot find feasible solution");
    }

    private SchedulerParameters GetSchedulerParameters(MonthScheduleParameters monthScheduleParameters, ScheduleFlexibility flexiblity)
    {
        return new SchedulerParameters(
            monthScheduleParameters.Month,
            monthScheduleParameters.EmployeeMonths,
            monthScheduleParameters.CompetenceGroups,
            monthScheduleParameters.LastMonthShifts,
            monthScheduleParameters.FixedShifts,
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
