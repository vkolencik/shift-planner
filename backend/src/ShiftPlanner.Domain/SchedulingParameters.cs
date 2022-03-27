namespace ShiftPlanner.Domain;

public record SchedulingParameters(
    IReadOnlyCollection<Day> Days,
    IReadOnlyCollection<Employee> Employees,
    IReadOnlyCollection<Workplace> Workplaces);
