using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class ScheduleValidationStrategy : IVariableFlexibilityStrategy<ScheduleValidationResult>
{
    private readonly IRulesetFactory _rulesetFactory;

    public ScheduleValidationStrategy(MonthScheduleParameters parameters, IRulesetFactory rulesetFactory)
    {
        _rulesetFactory = rulesetFactory;
        Parameters = parameters;
    }

    public bool PerformWithFlexibilityValue(ScheduleFlexibility flexibility, out ScheduleValidationResult result)
    {
        var rules = _rulesetFactory.CreateRuleset(Parameters, flexibility, new SchedulingParametersRecorder());
        var errors = new List<ScheduleValidationError>();

        foreach (var rule in rules)
        {
            rule.Apply(null);
        }

        result = new ScheduleValidationResult(errors);
        return !errors.Any();
    }

    public MonthScheduleParameters Parameters { get; }
}
