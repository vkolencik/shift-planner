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
        var dodo = new Employee("Dodo");

        var aro = new Workplace("aro");
        
        var schedule = scheduler.ComputeSchedule(
            new SchedulingParameters(
                new ScheduledMonth(2022, 2),
                new[] {pat, mat, dodo},
                new[] {aro}
            ));

        var patsShifts = schedule.Shifts
            .Where(s => s.Employee == pat)
            .ToArray();

        var minDaysBetweenShifts = patsShifts
            .OrderBy(s => s.Shift.Day.Date.ToDateTime(new TimeOnly(0, 0)))
            .Zip(patsShifts.Skip(1), (s1, s2) => DaysBetween(s1.Shift.Day.Date, s2.Shift.Day.Date))
            .Min();
        Assert.True(minDaysBetweenShifts >= 2);
        
        var aroShifts = schedule.Shifts
            .Where(s => s.Shift.Workplace == aro)
            .ToArray();
        Assert.Equal(28, aroShifts.Length);

        var daysWithShiftAndAlsoShiftTwoDaysAfter = patsShifts
            .OrderBy(s => s.Shift.Day.Date.ToDateTime(new TimeOnly(0, 0)))
            .Zip(patsShifts.Skip(1), (s1, s2) => Tuple.Create(s1.Shift.Day, DaysBetween(s1.Shift.Day.Date, s2.Shift.Day.Date)))
            .Where(x => x.Item2 == 2)
            .Select(x => x.Item1);
        
        Assert.All(daysWithShiftAndAlsoShiftTwoDaysAfter, day => Assert.Equal(DayOfWeek.Friday, day.Date.DayOfWeek));
    }

    private static int DaysBetween(DateOnly d1, DateOnly d2) =>
        d2.ToDateTime(new TimeOnly(0, 0)).Subtract(d1.ToDateTime(new TimeOnly(0, 0))).Days;

}

