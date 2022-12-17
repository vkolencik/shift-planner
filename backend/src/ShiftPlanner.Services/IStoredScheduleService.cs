using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Services;

public interface IStoredScheduleService
{
    Task<StoredSchedule> GetSchedule(ScheduledMonth month);
}
