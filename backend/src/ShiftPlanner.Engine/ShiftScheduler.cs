using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules;
using ShiftPlanner.Rules.Rules;

namespace ShiftPlanner.Engine;

public class ShiftScheduler
{
    public SchedulingResult ComputeSchedule(MonthScheduleParameters monthScheduleParameters)
    {
        var solverParameters = GetSolverParameters(monthScheduleParameters);
        var recorder = new SchedulingParametersRecorder();

        for (uint degreeOfFlexibility = 0; degreeOfFlexibility <= 50; degreeOfFlexibility++)
        {
            recorder.Reset();
            var flexibility = new ScheduleFlexibility(degreeOfFlexibility);
            var schedulerParameters = GetSchedulerParameters(monthScheduleParameters, flexibility);
            var engine = SchedulingEngine.Engine.CreateGoogleSolverEngine(solverParameters);

            var rules = RulesetFactory.CreateRuleset(schedulerParameters).ToList();
            rules.ForEach(r => r.Apply(engine));

            Rules.NoConsecutiveShiftsForEmployee(monthScheduleParameters, engine);

            Rules.NoAlternativeDayShiftsForEmployee(monthScheduleParameters, engine);

            Rules.LimitWeekendShifts(monthScheduleParameters, engine);

            Rules.AlwaysScheduleAlternatesOverWeekends(monthScheduleParameters, engine);
            
            Rules.AvoidSubsequentWeekends(monthScheduleParameters, engine);

            Rules.EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(
                monthScheduleParameters, solverParameters.Days, engine);

            Rules.DisallowShiftsWithoutEmployeeCapability(solverParameters, engine);

            EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulerParameters, engine, recorder);

            EmployeeStandardRule.AssignShiftsByPreferencePercentages(schedulerParameters, engine);

            EmployeeStandardRule.SpreadShiftsEvenlyThroughoutMonth(schedulerParameters, engine);

            Rules.TakeLeavesIntoAccount(monthScheduleParameters, engine);

            Rules.EnforceFixedShifts(monthScheduleParameters, engine);

            var result = engine.Solve();

            if (result != null)
            {
                return new SuccessfulSchedulingResult(result, flexibility, recorder.GetParameters);
            }
        }

        return new SolutionNotFound(recorder.GetParameters);
    }

    private SchedulerParameters GetSchedulerParameters(MonthScheduleParameters monthScheduleParameters, ScheduleFlexibility flexibility)
    {
        return new SchedulerParameters(
            monthScheduleParameters.Month,
            monthScheduleParameters.EmployeeMonths,
            monthScheduleParameters.CompetenceGroups,
            monthScheduleParameters.LastMonthShifts,
            monthScheduleParameters.FixedShifts,
            monthScheduleParameters.WeekendInterchangeabilityGroups,
            flexibility);
    }

    private static ISolverParameters GetSolverParameters(MonthScheduleParameters monthScheduleParameters) => monthScheduleParameters;
}
