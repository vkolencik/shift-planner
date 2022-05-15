namespace ShiftPlanner.Domain;

public record SchedulingParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups)
{
    public IReadOnlyCollection<Employee> Employees => EmployeeMonths.Select(em => em.Employee).ToArray();
}
