using System;
using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests;

public class ShiftSchedulerShould
{
    [Fact]
    public void ComputeSimpleSchedule()
    {
        var scheduler = new ShiftScheduler();

        var pat = new Employee("Pat");
        var mat = new Employee("Mat");

        var schedule = scheduler.ComputeSchedule(
            new SchedulingParameters(
                Enumerable.Range(1, 10).Select(d => new Day(new DateOnly(2022, 2, d))).ToArray(),
                new[] {pat, mat},
                new[] {new Workplace("aro")}
            ));

        var patsShifts = schedule.Shifts
            .Where(s => s.Employee == pat)
            .OrderBy(s => s.Day.Date.ToDateTime(new TimeOnly(0, 0)))
            .ToList();

        var minDaysBetweenShifts = patsShifts
            .Zip(patsShifts.Skip(1), (s1, s2) => DaysBetween(s1.Day.Date, s2.Day.Date))
            .Min();

        Assert.True(minDaysBetweenShifts >= 2);
    }

    private int DaysBetween(DateOnly d1, DateOnly d2) =>
        d2.ToDateTime(new TimeOnly(0, 0)).Subtract(d1.ToDateTime(new TimeOnly(0, 0))).Days;

}

