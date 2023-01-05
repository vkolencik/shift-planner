using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public interface IScheduleValidator
{
    Task<IReadOnlyCollection<ScheduleValidationError>> ValidateSchedule(Schedule schedule);
}
