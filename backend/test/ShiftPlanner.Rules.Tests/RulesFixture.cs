using ShiftPlanner.Domain;
using ShiftPlanner.TestHelpers;

namespace ShiftPlanner.Rules.Tests;

static internal class RulesFixture
{
    internal static readonly Day Day1 = new Day(2022, 2, 1);
    internal static readonly Day Day2 = new Day(2022, 2, 2);
    internal static readonly Day Day3 = new Day(2022, 2, 3);
    internal static readonly Day Day4Friday = new Day(2022, 2, 4);
    internal static readonly Day Day5Saturday = new Day(2022, 2, 5);
    internal static readonly Day Day6Sunday = new Day(2022, 2, 6);
    internal static readonly Day Day7 = new Day(2022, 2, 7);
    internal static readonly Workplace Workplace1 = new Workplace("Workplace1");
    internal static readonly Workplace Workplace2 = new Workplace("Workplace2");
    internal static readonly Capabilities CapabilitiesFirstWorkplace = new Capabilities(new WorkplaceCapability[] {new WorkplaceCapability.Green(Workplace1, 100), new WorkplaceCapability.Red(Workplace2)});
    internal static readonly Capabilities CapabilitiesBothWorkplaces = new Capabilities(new WorkplaceCapability[] {new WorkplaceCapability.Yellow(Workplace1), new WorkplaceCapability.Green(Workplace2, 100)});
    internal static readonly Employee Employee1 = new Employee("Employee1", CompetenceLevel.Level1, CapabilitiesFirstWorkplace, Workload.Infinite);
    internal static readonly Employee Employee2 = new Employee("Employee2", CompetenceLevel.Level2, CapabilitiesBothWorkplaces, Workload.Infinite);
    internal static readonly Employee Employee3 = new Employee("Employee3", CompetenceLevel.Level1, CapabilitiesBothWorkplaces, Workload.Infinite);

    internal static readonly ISolverParameters SolverParametersTwoDays = new SolverParameters(
        new[] {Day1, Day2},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

    internal static readonly ISolverParameters SolverParametersOneWeek = new SolverParameters(
        new[] {Day1, Day2, Day3, Day4Friday, Day5Saturday, Day6Sunday, Day7},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

    internal static readonly ScheduledMonth ScheduledMonth = new ScheduledMonth(2022, 2);
    
    internal static readonly MonthScheduleParameters MonthScheduleParameters = new MonthScheduleParameters(
        ScheduledMonth,
        new[]
        {
            new EmployeeMonth(Employee1, new[] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 2))}),
            new EmployeeMonth(Employee2),
            new EmployeeMonth(Employee3)
        },
        new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})},
        Array.Empty<ShiftAssignment>(),
        Array.Empty<ShiftAssignment>(),
        Array.Empty<ISet<Workplace>>());

}
