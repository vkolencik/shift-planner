using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Rules.Ruleset;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

internal class ScheduleValidationStrategy : IVariableFlexibilityStrategy<ScheduleValidationResult>
{
    private readonly Schedule _schedule;
    private readonly IRulesetFactory _rulesetFactory;
    private readonly IValidatingReceiverFactory _receiverFactory;

    public ScheduleValidationStrategy(Schedule schedule, MonthScheduleParameters parameters, IRulesetFactory rulesetFactory, IValidatingReceiverFactory receiverFactory)
    {
        _schedule = schedule;
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
            var errorType = ScheduleErrorType.Unknown;
            if (rule.GetType() ==  typeof(EveryDayAtEveryWorkplaceIsCoveredRule))
            {
                errorType = ScheduleErrorType.EveryDayIsCovered;
            }
            
            var receiver = _receiverFactory.GetReceiver(_schedule, errorType);
            
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
    IScheduleValidatingConstraintReceiver GetReceiver(Schedule schedule, ScheduleErrorType scheduleErrorType);
}
