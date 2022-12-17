namespace ShiftPlanner.Contracts;

public record Schedule(IReadOnlyList<ScheduledShift> Shifts);
