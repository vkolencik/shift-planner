using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts;

public record ScheduledShift(Day Day, Workplace Workplace, User User);
