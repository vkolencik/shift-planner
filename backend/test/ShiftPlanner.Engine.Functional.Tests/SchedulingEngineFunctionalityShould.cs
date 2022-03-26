using System;
using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests
{
    public class SchedulingEngineFunctionalityShould
    {
        [Fact]
        public void ComputeBasicSchedule()
        {

            var vojta = new Employee("Vojta");
            var aro = new Workplace("ARO");

            var inputParams = new SchedulingParameters(
                Day.CreateDaysForMonth(new ScheduledMonth(2022, 2)),
                new[] {vojta},
                new[] {aro}
            ); 

            var engine = SchedulingEngine.CreateGoogleSolverEngine(inputParams);

            // TODO: remove this and move to a separate layer
            for (int d = 1; d <= 28; d++)
            {
                engine.AddConstraint(new [] {new Shift(vojta, new Day(new DateOnly(2022, 2, d)), aro)}, Constraint.Eq, 1);
            }

            var schedule = engine.Solve();

            for (int d = 1; d <= 28; d++)
            {
                var day = new Day(new DateOnly(2022, 2, d));
                Assert.Equal(vojta, schedule.GetAssignedEmployee(day, aro));
            }
        }
    }
}
