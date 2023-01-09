using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

public record struct VariableFlexibilityCalculationResult<TResult>(
    TResult Result,
    bool IsSuccess,
    ScheduleFlexibility AttainedFlexibility);
