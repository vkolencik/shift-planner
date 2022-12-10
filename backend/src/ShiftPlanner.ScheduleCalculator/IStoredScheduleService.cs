using ShiftPlanner.Domain;

namespace ShiftPlanner.ScheduleCalculator;

public interface IStoredScheduleService
{
    Task<Schedule> GetSchedule(ScheduledMonth month);
}
