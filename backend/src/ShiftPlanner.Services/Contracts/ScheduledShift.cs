using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Contracts;

public record ScheduledShift(Day Day, Workplace Workplace, User User);
