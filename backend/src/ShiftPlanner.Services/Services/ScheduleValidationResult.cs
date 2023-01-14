using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public record ScheduleValidationResult(IReadOnlyCollection<ScheduleValidationError> Errors);
