using ShiftPlanner.Domain;
using ShiftPlanner.TestHelpers;

namespace ShiftPlanner.Engine.Tests;

static internal class RulesFixture
{
    public static readonly Day Day1 = new Day(2022, 2, 1);
    public static readonly Day Day2 = new Day(2022, 2, 2);
    public static readonly Day Day3 = new Day(2022, 2, 3);
    public static readonly Day Day4Friday = new Day(2022, 2, 4);
    public static readonly Day Day5Saturday = new Day(2022, 2, 5);
    public static readonly Day Day6Sunday = new Day(2022, 2, 6);
    public static readonly Day Day7 = new Day(2022, 2, 7);
    public static readonly Workplace Workplace1 = new Workplace("Workplace1");
    public static readonly Workplace Workplace2 = new Workplace("Workplace2");
    public static readonly Capabilities CapabilitiesFirstWorkplace = new Capabilities(new WorkplaceCapability[] {new WorkplaceCapability.Green(Workplace1, 100), new WorkplaceCapability.Red(Workplace2)});
    public static readonly Capabilities CapabilitiesBothWorkplaces = new Capabilities(new WorkplaceCapability[] {new WorkplaceCapability.Yellow(Workplace1), new WorkplaceCapability.Green(Workplace2, 100)});
    public static readonly Employee Employee1 = new Employee("Employee1", CompetenceLevel.Level1, CapabilitiesFirstWorkplace, Workload.Infinite);
    public static readonly Employee Employee2 = new Employee("Employee2", CompetenceLevel.Level2, CapabilitiesBothWorkplaces, Workload.Infinite);
    public static readonly Employee Employee3 = new Employee("Employee3", CompetenceLevel.Level1, CapabilitiesBothWorkplaces, Workload.Infinite);

    public static readonly ISolverParameters SolverParametersTwoDays = new SolverParameters(
        new[] {Day1, Day2},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

    public static readonly ISolverParameters SolverParametersOneWeek = new SolverParameters(
        new[] {Day1, Day2, Day3, Day4Friday, Day5Saturday, Day6Sunday, Day7},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

}
