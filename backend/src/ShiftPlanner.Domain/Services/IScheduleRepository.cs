namespace ShiftPlanner.Domain.Services;

public interface IScheduleRepository
{
    Task StoreComputedSchedule(ScheduledMonth month, SuccessfulSchedulingResult result);
}
