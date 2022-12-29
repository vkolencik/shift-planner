using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules;

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

            var rules = RulesetFactory.CreateRuleset(schedulerParameters, flexibility, recorder).ToList();
            rules.ForEach(r => r.Apply(engine));

            EmployeeStandardRule.SpreadShiftsEvenlyThroughoutMonth(schedulerParameters, engine);

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
