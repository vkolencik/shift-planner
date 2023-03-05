namespace ShiftPlanner.Domain;

public interface IMonthScheduleParameters : ISolverParameters
{
    ScheduledMonth Month { get; }
    IReadOnlyCollection<EmployeeMonth> EmployeeMonths { get; }
    IReadOnlyCollection<CompetenceGroup> CompetenceGroups { get; }
    IReadOnlyCollection<ShiftAssignment> LastMonthShifts { get; }
    IReadOnlyCollection<ShiftAssignment> FixedShifts { get; }
    IReadOnlyCollection<ISet<Workplace>> WeekendInterchangeabilityGroups { get; }
    IReadOnlyCollection<ShiftAssignment> ActiveEmployeeLastMonthShifts { get; }
}

public record MonthScheduleParameters(
        ScheduledMonth Month,
        IReadOnlyCollection<EmployeeMonth> EmployeeMonths,
        IReadOnlyCollection<CompetenceGroup> CompetenceGroups,
        IReadOnlyCollection<ShiftAssignment> LastMonthShifts,
        IReadOnlyCollection<ShiftAssignment> FixedShifts,
        IReadOnlyCollection<ISet<Workplace>> WeekendInterchangeabilityGroups)
    : IMonthScheduleParameters
{
    public IReadOnlyCollection<Day> Days => Month.Days;
    public IReadOnlyCollection<Employee> Employees => EmployeeMonths.Select(em => em.Employee).ToArray();

    public IReadOnlyCollection<Workplace> Workplaces => CompetenceGroups.SelectMany(cg => cg.Workplaces).ToArray();
    public IReadOnlyCollection<ShiftAssignment> ActiveEmployeeLastMonthShifts => LastMonthShifts.Where(lms => Employees.Contains(lms.Employee)).ToArray();
}
