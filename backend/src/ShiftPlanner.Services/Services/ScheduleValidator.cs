using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class ScheduleValidator : IScheduleValidator
{
    private readonly ISchedulingParametersAggregator _paramsAggregator;
    private readonly IValidationFlexibilityVariator _flexibilityVariator;
    private readonly IRulesetFactory _rulesetFactory;

    public ScheduleValidator(ISchedulingParametersAggregator paramsAggregator, IValidationFlexibilityVariator flexibilityVariator, IRulesetFactory rulesetFactory)
    {
        _paramsAggregator = paramsAggregator;
        _flexibilityVariator = flexibilityVariator;
        _rulesetFactory = rulesetFactory;
    }

    public async Task<IReadOnlyCollection<ScheduleValidationError>> ValidateSchedule(Tenant tenant, ScheduledMonth month, Schedule schedule)
    {
        var parameters = await _paramsAggregator.GetSchedulingParameters(tenant, month);
        
        var result = _flexibilityVariator.ExecuteWithVariableFlexibility(new ScheduleValidationStrategy(schedule, parameters, _rulesetFactory, new ReceiverFactory()));
        
        if (result.IsSuccess)
        {
            return Array.Empty<ScheduleValidationError>();
        }

        return result.Result.Errors;
    }

    private class ReceiverFactory : IValidatingReceiverFactory
    {
        public IScheduleValidatingConstraintReceiver GetReceiver(Schedule schedule, ScheduleErrorType scheduleErrorType) => new ScheduleValidatingConstraintReceiver(schedule, scheduleErrorType);
    }
}
