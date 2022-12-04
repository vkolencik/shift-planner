using ShiftPlanner.Domain;

namespace ShiftPlanner.ScheduleCalculator;

public interface ISchedulingParametersAggregator
{
    public Task<MonthScheduleParameters> GetSchedulingParameters(ScheduledMonth month);
}
