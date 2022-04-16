namespace ShiftPlanner.Domain;

public record SolverParameters(
    IReadOnlyCollection<Day> Days,
    IReadOnlyCollection<Employee> Employees,
    IReadOnlyCollection<Workplace> Workplaces);
