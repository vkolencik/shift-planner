using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public interface IScheduleValidator
{
    Task<IReadOnlyCollection<ScheduleValidationError>> ValidateSchedule(Tenant tenant, ScheduledMonth month, Schedule schedule);
}
