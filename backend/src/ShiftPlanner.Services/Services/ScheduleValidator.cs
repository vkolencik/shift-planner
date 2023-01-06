using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class ScheduleValidator : IScheduleValidator
{
    public Task<IReadOnlyCollection<ScheduleValidationError>> ValidateSchedule(Schedule schedule)
    {
        throw new NotImplementedException();
    }
}
