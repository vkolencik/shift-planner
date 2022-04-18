using System;
using System.Collections.Generic;
using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests;

public class ShiftSchedulerShould
{
    private readonly IReadOnlyCollection<Employee> _employees;

    private readonly IReadOnlyCollection<CompetenceGroup> _competenceGroups;

    public ShiftSchedulerShould()
    {
        _competenceGroups = new[]
        {
            new CompetenceGroup(new[] {new Workplace("aro1"), new Workplace("aro2")}),
            new CompetenceGroup(new[] {new Workplace("res1"), new Workplace("res2")})
        };

        var capableForAllWorkplaces = new Capabilities(_competenceGroups.SelectMany(cg => cg.Workplaces).ToArray());
        var capableForFirstWorkplace = new Capabilities(new[] {_competenceGroups.First().Workplaces.First()});
        
        _employees = Enumerable.Range(1, 48)
            .Select(i => new Employee(
                Name: $"Employee{i}",
                CompetenceLevel: (CompetenceLevel) (i % 4 + 1),
                Capabilities: i == 0 ? capableForFirstWorkplace : capableForAllWorkplaces
            )).ToArray();
    }

    [Fact]
    public void ComputeSimpleSchedule()
    {
        var scheduler = new ShiftScheduler();

        var schedule = scheduler.ComputeSchedule(
            new SchedulingParameters(
                new ScheduledMonth(2022, 2),
                _employees,
                _competenceGroups
            ));
        
        foreach (var employee in _employees)
        {
            AssertEmployeeInvariants(schedule, employee);
        }

        foreach (var competenceGroup in _competenceGroups)
        {
            AssertCompetenceGroupInvariants(schedule, competenceGroup);
        }
        
        foreach (var workplace in _competenceGroups.SelectMany(g => g.Workplaces))
        {
            AssertWorkplaceInvariants(schedule, workplace);
        }
    }

    private static void AssertEmployeeInvariants(Schedule schedule, Employee employee)
    {
        var employeeShifts = schedule.Shifts
            .Where(s => s.Employee == employee)
            .ToArray();

        if (!employeeShifts.Any())
        {
            return;
        }

        if (employeeShifts.Length > 1)
        {
            var minDaysBetweenShifts = employeeShifts
                .OrderBy(s => s.Shift.Day.Date.ToDateTime(new TimeOnly(0, 0)))
                .Zip(employeeShifts.Skip(1), (s1, s2) => DaysBetween(s1.Shift.Day.Date, s2.Shift.Day.Date))
                .Min();
            Assert.True(minDaysBetweenShifts >= 2);
        }

        if (employeeShifts.Length > 2)
        {
            var daysWithShiftAndAlsoShiftTwoDaysAfter = employeeShifts
                .OrderBy(s => s.Shift.Day.Date.ToDateTime(new TimeOnly(0, 0)))
                .Zip(
                    employeeShifts.Skip(1),
                    (s1, s2) => Tuple.Create(s1.Shift.Day, DaysBetween(s1.Shift.Day.Date, s2.Shift.Day.Date)))
                .Where(x => x.Item2 == 2)
                .Select(x => x.Item1);

            Assert.All(daysWithShiftAndAlsoShiftTwoDaysAfter, day => Assert.Equal(DayOfWeek.Friday, day.Date.DayOfWeek));
        }
    }

    private static void AssertCompetenceGroupInvariants(Schedule schedule, CompetenceGroup competenceGroup)
    {
        var competenceGroupShifts = schedule.Shifts
            .Where(s => competenceGroup.Workplaces.Contains(s.Shift.Workplace))
            .ToArray();

        var shiftsPerDay = Enumerable.Range(1, 28)
            .Select(d => competenceGroupShifts.Where(s => s.Shift.Day.Date.Day == d))
            .ToArray();
        
        Assert.All(shiftsPerDay, dayShifts => Assert.Contains(dayShifts, s => s.Employee.CompetenceLevel == CompetenceLevel.Level1));
    }

    private static void AssertWorkplaceInvariants(Schedule schedule, Workplace workplace)
    {
        var workplaceShifts = schedule.Shifts
            .Where(s => s.Shift.Workplace == workplace)
            .ToArray();
        Assert.Equal(28, workplaceShifts.Length);
        Assert.Equal(Enumerable.Range(1, 28), workplaceShifts.Select(s => s.Shift.Day.Date.Day));
    }

    private static int DaysBetween(DateOnly d1, DateOnly d2) =>
        d2.ToDateTime(new TimeOnly(0, 0)).Subtract(d1.ToDateTime(new TimeOnly(0, 0))).Days;
}
