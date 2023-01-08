using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class ScheduleValidator : IScheduleValidator
{
    private readonly ISchedulingParametersAggregator _paramsAggregator;

    public ScheduleValidator(ISchedulingParametersAggregator paramsAggregator)
    {
        _paramsAggregator = paramsAggregator;
    }

    public async Task<IReadOnlyCollection<ScheduleValidationError>> ValidateSchedule(Tenant tenant, ScheduledMonth month, Schedule schedule)
    {
        return Array.Empty<ScheduleValidationError>();
    }
}
