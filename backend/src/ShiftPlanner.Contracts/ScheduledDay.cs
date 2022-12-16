namespace ShiftPlanner.Contracts;

public record ScheduledDay(DateOnly Date, IReadOnlyList<ScheduledShift> Shifts);
