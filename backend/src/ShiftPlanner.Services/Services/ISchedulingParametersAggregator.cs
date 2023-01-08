using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Services;

public interface ISchedulingParametersAggregator
{
    public Task<MonthScheduleParameters> GetSchedulingParameters(Tenant tenant, ScheduledMonth month);
}
