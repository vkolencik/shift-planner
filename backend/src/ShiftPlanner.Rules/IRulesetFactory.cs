using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules;

public interface IRulesetFactory
{
    IEnumerable<IRule> CreateRuleset(
        IMonthScheduleParameters parameters,
        ScheduleFlexibility flexibility,
        ISchedulingParametersRecorder recorder);
}
