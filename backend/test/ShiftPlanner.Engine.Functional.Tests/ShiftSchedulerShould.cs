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

        var employees = Enumerable.Range(1,12).Select(i => new Employee($"Employee{i}")).ToArray();

        var workplaces = new[]
        {
            new Workplace("aro1"),
            new Workplace("aro2"),
            new Workplace("res1"),
            new Workplace("res2")
        };
        
        var e1 = employees[0];
        var aro1 = workplaces[0];

        var schedule = scheduler.ComputeSchedule(
            new SchedulingParameters(
                new ScheduledMonth(2022, 2),
                employees,
                workplaces
            ));
        
        var e1Shifts = schedule.Shifts
            .Where(s => s.Employee == e1)
            .ToArray();

        var minDaysBetweenShifts = e1Shifts
            .OrderBy(s => s.Shift.Day.Date.ToDateTime(new TimeOnly(0, 0)))
            .Zip(e1Shifts.Skip(1), (s1, s2) => DaysBetween(s1.Shift.Day.Date, s2.Shift.Day.Date))
            .Min();
        Assert.True(minDaysBetweenShifts >= 2);

        var aroShifts = schedule.Shifts
            .Where(s => s.Shift.Workplace == aro1)
            .ToArray();
        Assert.Equal(28, aroShifts.Length);
        Assert.Equal(Enumerable.Range(1, 28), aroShifts.Select(s => s.Shift.Day.Date.Day));

        var daysWithShiftAndAlsoShiftTwoDaysAfter = e1Shifts
            .OrderBy(s => s.Shift.Day.Date.ToDateTime(new TimeOnly(0, 0)))
            .Zip(
                e1Shifts.Skip(1),
                (s1, s2) => Tuple.Create(s1.Shift.Day, DaysBetween(s1.Shift.Day.Date, s2.Shift.Day.Date)))
            .Where(x => x.Item2 == 2)
            .Select(x => x.Item1);

        Assert.All(daysWithShiftAndAlsoShiftTwoDaysAfter, day => Assert.Equal(DayOfWeek.Friday, day.Date.DayOfWeek));
    }

    private static int DaysBetween(DateOnly d1, DateOnly d2) =>
        d2.ToDateTime(new TimeOnly(0, 0)).Subtract(d1.ToDateTime(new TimeOnly(0, 0))).Days;
}
