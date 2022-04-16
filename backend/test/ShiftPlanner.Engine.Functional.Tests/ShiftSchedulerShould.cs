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

        var aro = new Workplace("aro");
        var schedule = scheduler.ComputeSchedule(
            new SchedulingParameters(
                new ScheduledMonth(2022, 2),
                new[] {pat, mat},
                new[] {aro}
            ));

        var patsShifts = schedule.Shifts
            .Where(s => s.Employee == pat)
            .ToArray();

        var aroShifts = schedule.Shifts
            .Where(s => s.Workplace == aro)
            .ToArray();

        var minDaysBetweenShifts = patsShifts
            .OrderBy(s => s.Day.Date.ToDateTime(new TimeOnly(0, 0)))
            .Zip(patsShifts.Skip(1), (s1, s2) => DaysBetween(s1.Day.Date, s2.Day.Date))
            .Min();

        Assert.True(minDaysBetweenShifts >= 2);
        Assert.Equal(28, aroShifts.Length);
    }

    private int DaysBetween(DateOnly d1, DateOnly d2) =>
        d2.ToDateTime(new TimeOnly(0, 0)).Subtract(d1.ToDateTime(new TimeOnly(0, 0))).Days;

}

