using System;
using System.Collections.Generic;
using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests;

public class ComputedScheduleShould
{
    private readonly IReadOnlyCollection<CompetenceGroup> _competenceGroups;

    private readonly IReadOnlyCollection<EmployeeMonth> _employeeMonths;

    public IEnumerable<Employee> Employees => _employeeMonths.Select(e => e.Employee);

    public ComputedScheduleShould()
    {
        var aro1 = new Workplace("aro1");
        var aro2 = new Workplace("aro2");
        var res1 = new Workplace("res1");
        var res2 = new Workplace("res2");
        _competenceGroups = new[]
        {
            new CompetenceGroup(new[] {aro1, aro2}),
            new CompetenceGroup(new[] {res1, res2})
        };

        var capableForFirstWorkplaces = new Capabilities(new WorkplaceCapability[]
        {
            new WorkplaceCapability.Green(aro1, 50), new WorkplaceCapability.Green(res1, 50),
            new WorkplaceCapability.Red(aro2), new WorkplaceCapability.Red(res2),  
        });

        var capableForSecondWorkplaces = new Capabilities(new WorkplaceCapability[]
        {
            new WorkplaceCapability.Red(aro1), new WorkplaceCapability.Red(res1),
            new WorkplaceCapability.Green(aro2, 50), new WorkplaceCapability.Green(res2, 50),
        });
        
        _employeeMonths = Enumerable.Range(1, 48)
            .Select(i => new EmployeeMonth(new Employee(
                Name: $"Employee{i}",
                CompetenceLevel: (i % 2 == 0 ? CompetenceLevel.Level1 : CompetenceLevel.Level3),
                Capabilities: i % 2 == 0 ? capableForFirstWorkplaces : capableForSecondWorkplaces
            ))).ToArray();
    }

    [Fact]
    public void HaveAtLeastTwoDayDifferenceBetweenShifts()
    {
        var schedule = ComputeSchedule();

        foreach (var employee in Employees)
        {
            var employeeShifts = schedule.Shifts
                .Where(s => s.Employee == employee)
                .ToArray();
            
            if (employeeShifts.Length > 1)
            {
                var minDaysBetweenShifts = employeeShifts
                    .OrderBy(s => s.Shift.Day.Date.ToDateTime(new TimeOnly(0, 0)))
                    .Zip(employeeShifts.Skip(1), (s1, s2) => DaysBetween(s1.Shift.Day.Date, s2.Shift.Day.Date))
                    .Min();
                Assert.True(minDaysBetweenShifts >= 2);
            }
        }
    }

    [Fact]
    public void OnlyHaveAlternateDayShiftsOnWeekends()
    {
        var schedule = ComputeSchedule();

        foreach (var employee in Employees)
        {
            var employeeShifts = schedule.Shifts
                .Where(s => s.Employee == employee)
                .ToArray();

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
    }

    [Fact]
    public void SpreadAssignedShiftsEvenlyAmongEmployees()
    {
        var schedule = ComputeSchedule();

        var shiftNumbers = Employees.Select(e => schedule.Shifts.Count(s => s.Employee == e));

        var maxShiftsPerMonth = shiftNumbers.Max();
        var minShiftsPerMonth = shiftNumbers.Min();
        Assert.True(maxShiftsPerMonth - minShiftsPerMonth <= 2, $"Minimum number of shifts {minShiftsPerMonth} is too different from maximum {maxShiftsPerMonth}");
    }

    [Fact]
    public void AssignShiftsAccordingToEmployeeCapabilities()
    {
        var schedule = ComputeSchedule();

        foreach (var employee in Employees)
        {
            var workplacesEmployeeWorksAt = new HashSet<Workplace>(
                schedule.Shifts
                    .Where(s => s.Employee == employee)
                    .Select(s => s.Shift.Workplace));
            Assert.Subset(employee.Capabilities.AllowedWorkplaces, workplacesEmployeeWorksAt);
        }
    }

    [Fact]
    public void HaveAtLeastOneLevel1EmployeeForEachCompetenceGroupAtAllTimes()
    {
        var schedule = ComputeSchedule();

        foreach (var competenceGroup in _competenceGroups)
        {
            var competenceGroupShifts = schedule.Shifts
                .Where(s => competenceGroup.Workplaces.Contains(s.Shift.Workplace))
                .ToArray();

            var shiftsPerDay = Enumerable.Range(1, 28)
                .Select(d => competenceGroupShifts.Where(s => s.Shift.Day.Date.Day == d))
                .ToArray();
        
            Assert.All(shiftsPerDay, dayShifts => Assert.Contains(dayShifts, s => s.Employee.CompetenceLevel == CompetenceLevel.Level1));
        }
    }

    [Fact]
    public void HaveEveryShiftAssignedToSomeone()
    {
        var schedule = ComputeSchedule();

        foreach (var workplace in _competenceGroups.SelectMany(g => g.Workplaces))
        {
            var workplaceShifts = schedule.Shifts
                .Where(s => s.Shift.Workplace == workplace)
                .ToArray();
            Assert.Equal(28, workplaceShifts.Length);
            Assert.Equal(Enumerable.Range(1, 28), workplaceShifts.Select(s => s.Shift.Day.Date.Day));
        }
    }

    private Schedule ComputeSchedule()
    {
        var scheduler = new ShiftScheduler();

        var schedule = scheduler.ComputeSchedule(
            new SchedulingParameters(
                new ScheduledMonth(2022, 2),
                _employeeMonths,
                _competenceGroups
            ));
        return schedule;
    }

    private static int DaysBetween(DateOnly d1, DateOnly d2) =>
        d2.ToDateTime(new TimeOnly(0, 0)).Subtract(d1.ToDateTime(new TimeOnly(0, 0))).Days;
}
