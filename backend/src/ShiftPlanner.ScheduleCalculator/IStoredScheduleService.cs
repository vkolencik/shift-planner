using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.ScheduleCalculator;

public interface IStoredScheduleService
{
    Task<StoredSchedule> GetSchedule(ScheduledMonth month);
}
