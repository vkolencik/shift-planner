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
            var engine = SchedulingEngine.Engine.CreateGoogleSolverEngine(solverParameters);

            var rules = RulesetFactory.CreateRuleset(monthScheduleParameters, flexibility, recorder).ToList();
            rules.ForEach(r => r.Apply(engine));

            var result = engine.Solve();

            if (result != null)
            {
                return new SuccessfulSchedulingResult(result, flexibility, recorder.GetParameters);
            }
        }

        return new SolutionNotFound(recorder.GetParameters);
    }

    private static ISolverParameters GetSolverParameters(MonthScheduleParameters monthScheduleParameters) => monthScheduleParameters;
}
