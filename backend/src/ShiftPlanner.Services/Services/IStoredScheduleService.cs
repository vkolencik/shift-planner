using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public interface IStoredScheduleService
{
    Task<Schedule?> GetSchedule(ScheduledMonth month);
}
