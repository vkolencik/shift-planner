namespace ShiftPlanner.Domain;

public record SchedulingParameters(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups)
{
    public IReadOnlyCollection<Employee> Employees => EmployeeMonths.Select(em => em.Employee).ToArray();

    public IReadOnlyCollection<Workplace> Workplaces => CompetenceGroups.SelectMany(cg => cg.Workplaces).ToArray();
}

public record SchedulingParameters2(
    ScheduledMonth Month,
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups,
    ScheduleFlexibility Flexibility)
{
    public IReadOnlyCollection<Employee> Employees => EmployeeMonths.Select(em => em.Employee).ToArray();

    public IReadOnlyCollection<Workplace> Workplaces => CompetenceGroups.SelectMany(cg => cg.Workplaces).ToArray();
}
