using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts.Services;

public interface IStoredScheduleService
{
    Task<Schedule> GetSchedule(ScheduledMonth month);
}
