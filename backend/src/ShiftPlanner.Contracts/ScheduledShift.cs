using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts;

public record ScheduledShift(Workplace Workplace, string UserId);
