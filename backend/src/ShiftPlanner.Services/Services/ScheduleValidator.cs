using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class ScheduleValidator : IScheduleValidator
{
    private readonly ISchedulingParametersAggregator _paramsAggregator;
    private readonly IFlexibilityVariator _flexibilityVariator;
    private readonly IRulesetFactory _rulesetFactory;

    public ScheduleValidator(ISchedulingParametersAggregator paramsAggregator, IFlexibilityVariator flexibilityVariator, IRulesetFactory rulesetFactory)
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
        public IScheduleValidatingConstraintReceiver GetReceiver(Schedule schedule) => new ScheduleValidatingConstraintReceiver(schedule);
    }
}

internal class ScheduleValidatingConstraintReceiver : IScheduleValidatingConstraintReceiver
{
    private readonly Schedule _schedule;

    public ScheduleValidatingConstraintReceiver(Schedule schedule)
    {
        _schedule = schedule;
    }

    public void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound)
    {
        throw new NotImplementedException();
    }

    public void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, int value)
    {
        throw new NotImplementedException();
    }

    public void AddSumRelationConstraint(IReadOnlyCollection<ShiftAssignment> shifts1, Constraint constraint, IReadOnlyCollection<ShiftAssignment> shifts2)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<ScheduleValidationError> GetErrors()
    {
        throw new NotImplementedException();
    }
}
