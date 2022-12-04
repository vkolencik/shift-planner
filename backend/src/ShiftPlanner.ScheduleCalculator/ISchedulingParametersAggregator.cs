using ShiftPlanner.Domain;

namespace ShiftPlanner.ScheduleCalculator;

public interface ISchedulingParametersAggregator
{
    public Task<MonthScheduleParameters> GetSchedulingParameters(Tenant tenant, ScheduledMonth month);
}
