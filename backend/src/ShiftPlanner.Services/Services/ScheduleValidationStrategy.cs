using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

internal class ScheduleValidationStrategy : IVariableFlexibilityStrategy<ScheduleValidationResult>
{
    private readonly IRulesetFactory _rulesetFactory;
    private readonly IValidatingReceiverFactory _receiverFactory;

    public ScheduleValidationStrategy(MonthScheduleParameters parameters, IRulesetFactory rulesetFactory, IValidatingReceiverFactory receiverFactory)
    {
        _rulesetFactory = rulesetFactory;
        _receiverFactory = receiverFactory;
        Parameters = parameters;
    }

    public bool PerformWithFlexibilityValue(ScheduleFlexibility flexibility, out ScheduleValidationResult result)
    {
        var rules = _rulesetFactory.CreateRuleset(Parameters, flexibility, new SchedulingParametersRecorder());
        var errors = new List<ScheduleValidationError>();

        foreach (var rule in rules)
        {
            var receiver = _receiverFactory.GetReceiver();
            rule.Apply(receiver);
            errors.AddRange(receiver.GetErrors());
        }

        result = new ScheduleValidationResult(errors);
        return !errors.Any();
    }

    public MonthScheduleParameters Parameters { get; }
}

public interface IScheduleValidatingConstraintReceiver : IScheduleConstraintReceiver
{
    IReadOnlyCollection<ScheduleValidationError> GetErrors();
}

public interface IValidatingReceiverFactory
{
    IScheduleValidatingConstraintReceiver GetReceiver();
}
