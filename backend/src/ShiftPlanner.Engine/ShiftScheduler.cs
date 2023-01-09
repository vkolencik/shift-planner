using ShiftPlanner.Domain;
using ShiftPlanner.Rules;

namespace ShiftPlanner.Engine;

public class ShiftScheduler
{
    public SchedulingResult ComputeSchedule(MonthScheduleParameters monthScheduleParameters)
    {
        var flexibilityVariator = new SequenceFlexibilityVariator<SchedulingResult>(new ScheduleFlexibility(50));

        var result = flexibilityVariator.ExecuteWithVariableFlexibility(new SolverSchedulingStrategy(monthScheduleParameters));

        return result.Result;
    }

    private class SolverSchedulingStrategy : IVariableFlexibilityStrategy<SchedulingResult>
    {
        private readonly MonthScheduleParameters _parameters;
        private readonly SchedulingParametersRecorder _recorder = new SchedulingParametersRecorder();

        public SolverSchedulingStrategy(MonthScheduleParameters parameters)
        {
            _parameters = parameters;
        }

        public bool PerformWithFlexibilityValue(ScheduleFlexibility flexibility, out SchedulingResult result)
        {
            _recorder.Reset();
            var engine = SchedulingEngine.Engine.CreateGoogleSolverEngine(_parameters);

            var rules = RulesetFactory.CreateRuleset(_parameters, flexibility, _recorder).ToList();
            rules.ForEach(r => r.Apply(engine));

            var scheduleResult = engine.Solve();
            
            result = scheduleResult is not null
                ? new SuccessfulSchedulingResult(scheduleResult, flexibility, _recorder.GetParameters)
                : new SolutionNotFound(_recorder.GetParameters);

            return scheduleResult is not null;
        }
    }
}

