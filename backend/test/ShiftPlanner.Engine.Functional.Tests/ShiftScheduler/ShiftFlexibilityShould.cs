using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests;

public class ShiftFlexibilityShould
{
    [Fact]
    public void MakeSchedulerUltimatelyFindASolution()
    {
        var scheduler = new ShiftScheduler();

        var workplaceA = new Workplace("A");
        var workplaceB = new Workplace("B");
        SchedulingParameters parameters = new SchedulingParameters(
            new ScheduledMonth(2022, 2),
            Enumerable.Range(1, 5).Select(
                    i => new EmployeeMonth(
                        new Employee(
                            $"EmployeeA{i}",
                            CompetenceLevel.Level1,
                            new Capabilities(
                                new WorkplaceCapability[]
                                {
                                    new WorkplaceCapability.Green(workplaceA, 100),
                                    new WorkplaceCapability.Red(workplaceB)
                                }))))
                .Concat(
                    Enumerable.Range(1, 15).Select(
                        i => new EmployeeMonth(
                            new Employee(
                                $"EmployeeB{i}",
                                CompetenceLevel.Level1,
                                new Capabilities(
                                    new WorkplaceCapability[]
                                    {
                                        new WorkplaceCapability.Red(workplaceA),
                                        new WorkplaceCapability.Green(workplaceB, 100)
                                    }))))
                ).ToArray(),
            new[] {new CompetenceGroup(new[] {workplaceA, workplaceB})},
            ScheduleFlexibility.Strict);

        var schedule = scheduler.ComputeSchedule(parameters);

        Assert.NotNull(schedule);
        Assert.NotEqual(ScheduleFlexibility.Strict, schedule.Flexibility);
    }
}
