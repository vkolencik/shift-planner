namespace ShiftPlanner.Domain;

public record Employee(string Name);

public record Workplace(string Name);

public record ScheduledMonth(int Year, int Month);

public record SchedulingParameters(
    IReadOnlyCollection<Day> Days,
    IReadOnlyCollection<Employee> Employees,
    IReadOnlyCollection<Workplace> Workplaces);
