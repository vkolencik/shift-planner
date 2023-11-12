using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules;

public class RulesetFactory : IRulesetFactory
{
    public IEnumerable<IRule> CreateRuleset(
        IMonthScheduleParameters parameters,
        ScheduleFlexibility flexibility,
        ISchedulingParametersRecorder recorder)
    {
        yield return new EveryDayAtEveryWorkplaceIsCoveredRule(parameters);
        yield return new NoConcurrentShiftsRule(parameters);

        yield return new NoConsecutiveShiftsRule(parameters);

        yield return new AlwaysScheduleAlternatesOverWeekendsRule(parameters);
        yield return new NoSubsequentWeekendsRule(parameters);
        yield return new WeekendShiftLimitRule(parameters);
        // yield return new MinNumberOfWeekendShiftsRule(parameters);

        yield return new NoAlternativeDayShiftsRule(parameters);
        // yield return new NoDoubleAlternativeDayShiftsRule(parameters);

        yield return new LeaveRule(parameters);
        
        yield return new WorkplaceCapabilityRule(parameters);
        yield return new CompetenceGroupQualificationRule(parameters);
        
        yield return new PredefinedShiftsRule(parameters);
        yield return new ExternalEmployeeRule(parameters);

        IEmployeeStandardCalculator calculator = new EmployeeStandardCalculator(flexibility, parameters);

        yield return new MonthlyShiftCountRule(calculator, parameters, recorder);
        // yield return new WorkplacePreferencePercentagesRule(calculator, parameters);
        // yield return new EvenSpreadWithinMonthRule(calculator, parameters);
    }
}
