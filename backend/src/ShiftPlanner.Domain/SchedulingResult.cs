namespace ShiftPlanner.Domain;

public abstract record SchedulingResult(SolutionParameters SolutionParameters);

public record SolutionNotFound(SolutionParameters SolutionParameters) : SchedulingResult(SolutionParameters);

public record SuccessfulSchedulingResult(PlannedSchedule PlannedSchedule, ScheduleFlexibility Flexibility, SolutionParameters SolutionParameters) : SchedulingResult(SolutionParameters);
