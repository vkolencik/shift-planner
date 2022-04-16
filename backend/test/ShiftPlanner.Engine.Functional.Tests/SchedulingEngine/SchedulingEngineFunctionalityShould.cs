using System;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests.SchedulingEngine
{
    public class SchedulingEngineFunctionalityShould
    {
        [Fact]
        public void ComputeBasicSchedule()
        {

            var vojta = new Employee("Vojta");
            var aro = new Workplace("ARO");

            var inputParams = new SolverParameters(
                Day.CreateDaysForMonth(new ScheduledMonth(2022, 2)),
                new[] {vojta},
                new[] {aro}
            );

            var engine = Engine.SchedulingEngine.SchedulingEngine.CreateGoogleSolverEngine(inputParams);

            for (int d = 1; d <= 28; d++)
            {
                engine.AddSumConstraint(new [] {new ShiftAssignment(vojta, new Day(new DateOnly(2022, 2, d)), aro)}, Constraint.Eq, 1);
            }

            var schedule = engine.Solve();

            for (int d = 1; d <= 28; d++)
            {
                var day = new Day(new DateOnly(2022, 2, d));
                Assert.Equal(vojta, schedule.GetAssignedEmployee(new Shift(day, aro)));
            }
        }
    }
}
