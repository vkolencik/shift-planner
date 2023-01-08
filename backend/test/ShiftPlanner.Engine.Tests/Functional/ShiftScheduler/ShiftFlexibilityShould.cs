using System;
using System.Collections.Generic;
using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Tests.Functional.ShiftScheduler;

public class ShiftFlexibilityShould
{
    [Fact]
    public void MakeSchedulerUltimatelyFindASolution()
    {
        var scheduler = new Engine.ShiftScheduler();

        var workplaceA = new Workplace("A");
        var workplaceB = new Workplace("B");
        var parameters = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            Enumerable.Range(1, 4).Select(
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
                    Enumerable.Range(1, 8).Select(
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
            Array.Empty<ShiftAssignment>(), Array.Empty<ShiftAssignment>(), Array.Empty<ISet<Workplace>>());

        var result = scheduler.ComputeSchedule(parameters);

        var successResult = Assert.IsType<SuccessfulSchedulingResult>(result);
        Assert.NotEmpty(successResult.PlannedSchedule.Shifts);
        Assert.NotEqual(ScheduleFlexibility.Strict, successResult.Flexibility);
    }
}
