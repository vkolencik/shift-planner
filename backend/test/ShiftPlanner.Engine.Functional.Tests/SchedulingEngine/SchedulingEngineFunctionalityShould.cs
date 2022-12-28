using System;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using ShiftPlanner.TestHelpers;
using ShiftScheduler.Rules;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests.SchedulingEngine
{
    public class SchedulingEngineFunctionalityShould
    {
        [Fact]
        public void ComputeBasicSchedule()
        {
            var aro = new Workplace("ARO");
            var vojta = new Employee("Vojta", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(aro, 100)}));

            var inputParams = new SolverParameters(
                new ScheduledMonth(2022, 2).Days,
                new[] {vojta},
                new[] {aro});

            var engine = ShiftPlanner.Engine.SchedulingEngine.Engine.CreateGoogleSolverEngine(inputParams);

            for (int d = 1; d <= 28; d++)
            {
                engine.AddSumConstraint(new[] {new ShiftAssignment(vojta, new Day(new DateOnly(2022, 2, d)), aro)}, Constraint.Eq, 1);
            }

            var result = engine.Solve();

            var schedule = Assert.IsType<PlannedSchedule>(result);

            for (int d = 1; d <= 28; d++)
            {
                var day = new Day(2022, 2, d);
                Assert.Equal(vojta, schedule.GetAssignedEmployee(new Shift(day, aro)));
            }
        }
    }
}
