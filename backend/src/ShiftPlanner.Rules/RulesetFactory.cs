using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Rules;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules;

public static class RulesetFactory
{
    public static IEnumerable<IRule> CreateRuleset(MonthScheduleParameters parameters)
    {
        yield return new EveryDayAtEveryWorkplaceIsCoveredRule(parameters);
        yield return new NoConcurrentShiftsRule(parameters);
        yield return new NoConsecutiveShiftsRule(parameters);
    }
}
