namespace ShiftPlanner.Domain;

public record SchedulingParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<Employee> Employees,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups);
