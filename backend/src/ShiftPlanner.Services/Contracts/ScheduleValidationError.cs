using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Contracts;

public record ScheduleValidationError(IReadOnlyCollection<Shift> Shifts, string? UserName, ScheduleErrorType ErrorType);

public enum ScheduleErrorType
{
    Unknown,
    EveryDayIsCovered,
    NoConcurrentShifts,
    NoConsecutiveShifts,
    NoAlternativeDayShifts,
    AlwaysAlternatesOverWeekends,
    WeekendLimit,
    MinNumberOfWeekends,
    NoSubsequentWeekends,
    Leaves,
    WorkplaceCapability,
    CompetenceGroupQualification,
    PredefinedShifts,
    MonthlyShiftCount,
    WorkplacePreferencePercentage,
    EvenSpreadWithinMonth
}
