using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Persistence;

public interface IScheduleRepository
{
    Task StoreComputedSchedule(ScheduledMonth month, SuccessfulSchedulingResult result);
    Task<Schedule?> LoadSchedule(ScheduledMonth month);
}
