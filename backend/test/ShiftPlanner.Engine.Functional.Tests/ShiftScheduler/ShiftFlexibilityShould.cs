using System;
using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests.ShiftScheduler;

public class ShiftFlexibilityShould
{
    [Fact]
    public void MakeSchedulerUltimatelyFindASolution()
    {
        var scheduler = new Engine.ShiftScheduler();

        var workplaceA = new Workplace("A");
        var workplaceB = new Workplace("B");
        MonthScheduleParameters parameters = new MonthScheduleParameters(
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
                                }),
                            Workload.Infinite
                        )))
                .Concat(
                    Enumerable.Range(1, 20).Select(
                        i => new EmployeeMonth(
                            new Employee(
                                $"EmployeeB{i}",
                                CompetenceLevel.Level1,
                                new Capabilities(
                                    new WorkplaceCapability[]
                                    {
                                        new WorkplaceCapability.Red(workplaceA),
                                        new WorkplaceCapability.Green(workplaceB, 100)
                                    }),
                                Workload.Infinite)))
                ).ToArray(),
            new[] {new CompetenceGroup(new[] {workplaceA, workplaceB})},
            Array.Empty<ShiftAssignment>(), Array.Empty<ShiftAssignment>());

        var result = scheduler.ComputeSchedule(parameters);

        var successResult = Assert.IsType<SuccessfulSchedulingResult>(result);
        Assert.NotEmpty(successResult.Schedule.Shifts);
        Assert.NotEqual(ScheduleFlexibility.Strict, successResult.Flexibility);
    }
}
