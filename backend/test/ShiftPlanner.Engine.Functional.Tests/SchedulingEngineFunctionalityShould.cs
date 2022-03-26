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

            var schedule = engine.Solve();

            for (int d = 1; d <= 28; d++)
            {
                var day = new DateOnly(2022, 2, d);
                Assert.Equal(vojta, schedule.GetAssignedEmployee(day, aro));
            }
        }
    }
}
