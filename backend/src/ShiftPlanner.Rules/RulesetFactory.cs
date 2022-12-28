using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Rules;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules;

public static class RulesetFactory
{
    public static IEnumerable<IRule> CreateRuleset(IMonthScheduleParameters parameters)
    {
        yield return new EveryDayAtEveryWorkplaceIsCoveredRule(parameters);
        yield return new NoConcurrentShiftsRule(parameters);
        
        yield return new NoConsecutiveShiftsRule(parameters);
        yield return new NoAlternativeDayShiftsRule(parameters);
        
        yield return new AlwaysScheduleAlternatesOverWeekendsRule(parameters);
        yield return new WeekendShiftLimitRule(parameters);
        yield return new NoSubsequentWeekendsRule(parameters);
    }
}
