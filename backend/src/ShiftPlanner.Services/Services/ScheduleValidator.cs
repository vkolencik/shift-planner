using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class ScheduleValidator : IScheduleValidator
{
    private readonly ISchedulingParametersAggregator _paramsAggregator;
    private readonly IFlexibilityVariator _flexibilityVariator;

    public ScheduleValidator(ISchedulingParametersAggregator paramsAggregator, IFlexibilityVariator flexibilityVariator)
    {
        _paramsAggregator = paramsAggregator;
        _flexibilityVariator = flexibilityVariator;
    }

    public async Task<IReadOnlyCollection<ScheduleValidationError>> ValidateSchedule(Tenant tenant, ScheduledMonth month, Schedule schedule)
    {
        var parameters = await _paramsAggregator.GetSchedulingParameters(tenant, month);
        
        var result = _flexibilityVariator.ExecuteWithVariableFlexibility(new ScheduleValidationStrategy(parameters));
        
        if (result.IsSuccess)
        {
            return Array.Empty<ScheduleValidationError>();
        }

        return result.Result.Errors;
    }

    public class ScheduleValidationStrategy : IVariableFlexibilityStrategy<ScheduleValidationResult>
    {
        public ScheduleValidationStrategy(MonthScheduleParameters parameters)
        {
            Parameters = parameters;
        }

        public bool PerformWithFlexibilityValue(ScheduleFlexibility flexibility, out ScheduleValidationResult result)
        {
            throw new NotImplementedException();
        }

        public MonthScheduleParameters Parameters { get; }
    }
}
