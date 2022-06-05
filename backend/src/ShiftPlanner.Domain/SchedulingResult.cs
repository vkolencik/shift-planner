namespace ShiftPlanner.Domain;

public abstract record SchedulingResult;

public record SolutionNotFound : SchedulingResult;

public record SuccessfulSchedulingResult(Schedule Schedule, ScheduleFlexibility Flexibility) : SchedulingResult;
