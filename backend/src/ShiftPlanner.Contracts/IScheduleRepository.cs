using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts;

public interface IScheduleRepository
{
    Task StoreComputedSchedule(ScheduledMonth month, SuccessfulSchedulingResult result);
    Task<StoredSchedule> LoadLatestSchedule(ScheduledMonth month);
}
