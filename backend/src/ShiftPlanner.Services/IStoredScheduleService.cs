using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Services;

public interface IStoredScheduleService
{
    Task<Schedule> GetSchedule(ScheduledMonth month);
}
