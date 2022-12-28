using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts;

public interface IScheduleRepository
{
    Task StoreComputedSchedule(ScheduledMonth month, SuccessfulSchedulingResult result);
    Task<Schedule?> LoadSchedule(ScheduledMonth month);
}
