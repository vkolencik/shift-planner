namespace ShiftPlanner.Domain;

public interface ISolverParameters
{
    IReadOnlyCollection<Day> Days { get; }
    IReadOnlyCollection<Employee> Employees { get; }
    IReadOnlyCollection<Workplace> Workplaces { get; }
}
