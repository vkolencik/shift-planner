namespace ShiftPlanner.Contracts;

public record StoredSchedule(IReadOnlyList<ScheduledDay> Schedule);
