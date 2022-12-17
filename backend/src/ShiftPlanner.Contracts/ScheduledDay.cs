using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts;

public record ScheduledDay(Day Day, IReadOnlyList<ScheduledShift> Shifts);
