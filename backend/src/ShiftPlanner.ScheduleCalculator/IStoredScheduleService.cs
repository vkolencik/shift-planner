using ShiftPlanner.Domain;

namespace ShiftPlanner.ScheduleCalculator;

public interface IStoredScheduleService
{
    Task<IReadOnlyCollection<ShiftAssignment>> GetSchedule(ScheduledMonth month);
}
