namespace ShiftPlanner.Domain;

public record EmployeeMonth(Employee Employee, IReadOnlyCollection<Leave> Leaves)
{
    public EmployeeMonth(Employee employee) : this(employee, Array.Empty<Leave>()) {}
}
