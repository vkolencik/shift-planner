namespace ShiftPlanner.Contracts;

public record StoredSchedule(IReadOnlyList<ScheduledShift> Schedule);
