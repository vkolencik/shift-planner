namespace ShiftPlanner.Domain;

public record MonthScheduleParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups)
{
    public IReadOnlyCollection<Employee> Employees => EmployeeMonths.Select(em => em.Employee).ToArray();

    public IReadOnlyCollection<Workplace> Workplaces => CompetenceGroups.SelectMany(cg => cg.Workplaces).ToArray();
}
