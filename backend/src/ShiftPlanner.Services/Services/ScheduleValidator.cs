using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class ScheduleValidator : IScheduleValidator
{
    public async Task<IReadOnlyCollection<ScheduleValidationError>> ValidateSchedule(Schedule schedule)
    {
        return Array.Empty<ScheduleValidationError>();
    }
}
