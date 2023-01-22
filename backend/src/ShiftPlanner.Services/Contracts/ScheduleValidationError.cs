using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Contracts;

public record ScheduleValidationError(IReadOnlyCollection<Shift> Shifts, User? User, ScheduleErrorType ErrorType);

public enum ScheduleErrorType
{
    Unknown,
    EveryDayIsCovered,
    NoConcurrentShifts
}
