using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Rules;

namespace ShiftPlanner.Rules;

public static class RulesetFactory
{
    public static IEnumerable<IRule> CreateRuleset(MonthScheduleParameters parameters)
    {
        yield return new EveryDayAtEveryWorkplaceIsCoveredRule(parameters);
    }
}
