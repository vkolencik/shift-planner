namespace ShiftPlanner.Services.Contracts;

public record Schedule(IReadOnlyList<ScheduledShift> Shifts);
