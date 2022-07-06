using System;
using System.Collections.Generic;
using System.Linq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Functional.Tests;

public class ComputedScheduleShould
{
    private readonly IReadOnlyCollection<EmployeeMonth> _employeeMonths;

    private IEnumerable<Employee> Employees => _employeeMonths.Select(e => e.Employee);
    private readonly int _employeesWithTimeOffCount = 3;

    static readonly Workplace Aro1 = new Workplace("aro1");
    static readonly Workplace Aro2 = new Workplace("aro2");
    static readonly Workplace Aro3 = new Workplace("aro3");
    static readonly Workplace Res1 = new Workplace("res1");
    static readonly Workplace Res2 = new Workplace("res2");
    static readonly Workplace Res3 = new Workplace("res3");
        
    private readonly IReadOnlyCollection<CompetenceGroup> _competenceGroups = new[]
    {
        new CompetenceGroup(new[] {Aro1, Aro2, Aro3}),
        new CompetenceGroup(new[] {Res1, Res2, Res3})
    };

    readonly EmployeeMonth _percentualCapabilitiesEmployee1 = new EmployeeMonth(
        new Employee(
            "PercentualCapabilitiesEmployee1",
            CompetenceLevel.Level1,
            new Capabilities(
                new WorkplaceCapability[]
                {
                    new WorkplaceCapability.Green(Aro1, 20), new WorkplaceCapability.Green(Res1, 20),
                    new WorkplaceCapability.Green(Aro2, 20), new WorkplaceCapability.Red(Res2),
                    new WorkplaceCapability.Green(Aro3, 40), new WorkplaceCapability.Red(Res3)
                })
        ));

    readonly EmployeeMonth _percentualCapabilitiesEmployee2WithLeave = new EmployeeMonth(
        new Employee(
            "PercentualCapabilitiesEmployee2",
            CompetenceLevel.Level2,
            new Capabilities(
                new WorkplaceCapability[]
                {
                    new WorkplaceCapability.Red(Aro1), new WorkplaceCapability.Red(Res1),
                    new WorkplaceCapability.Red(Aro2), new WorkplaceCapability.Red(Res2),
                    new WorkplaceCapability.Green(Aro3, 80), new WorkplaceCapability.Green(Res3, 20),
                })
        ),
        new[] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 15))}
    );

    private IEnumerable<EmployeeMonth> EmployeeMonthsWithTimeOff => _employeeMonths.Take(_employeesWithTimeOffCount).Concat(new [] { _percentualCapabilitiesEmployee2WithLeave});    
    private IEnumerable<Employee> EmployeesWithTimeOff => EmployeeMonthsWithTimeOff.Select(em => em.Employee);
    private IEnumerable<Employee> EmployeesWithoutTimeOff => Employees.Except(EmployeesWithTimeOff);
    public ComputedScheduleShould()
    {
        var capableForFirstWorkplaces = new Capabilities(new WorkplaceCapability[]
        {
            new WorkplaceCapability.Green(Aro1, 30), new WorkplaceCapability.Green(Res1, 40),
            new WorkplaceCapability.Green(Aro2, 30), new WorkplaceCapability.Red(Res2),  
            new WorkplaceCapability.Red(Aro3), new WorkplaceCapability.Red(Res3),  
        });

        var capableForSecondWorkplaces = new Capabilities(new WorkplaceCapability[]
        {
            new WorkplaceCapability.Red(Aro1), new WorkplaceCapability.Red(Res1),
            new WorkplaceCapability.Red(Aro2), new WorkplaceCapability.Green(Res2, 40),
            new WorkplaceCapability.Green(Aro3, 30), new WorkplaceCapability.Green(Res3, 30),
        });
       
        _employeeMonths = Enumerable.Range(1, 40)
            .Select(i => new EmployeeMonth(
                new Employee(
                    Name: $"Employee{i}",
                    CompetenceLevel: (i % 2 == 0 ? CompetenceLevel.Level1 : CompetenceLevel.Level3),
                    Capabilities: i % 2 == 0 ? capableForFirstWorkplaces : capableForSecondWorkplaces),
                i <= _employeesWithTimeOffCount ? new [] { new Leave(new Day(2022, 2, 1 + i * 2), new Day(2022, 2, 1 + i * 2 + 14)) } : Array.Empty<Leave>()                
            ))
            .Concat(new[] {_percentualCapabilitiesEmployee1, _percentualCapabilitiesEmployee2WithLeave})
            .ToArray();
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

                Assert.All(daysWithShiftAndAlsoShiftTwoDaysAfter, day => Assert.Equal(DayOfWeek.Friday, day.DayOfWeek));
            }
        }
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

    [Fact]
    public void SpreadAssignedShiftsEvenlyAmongEmployees()
    {
        var schedule = ComputeSchedule();

        var shiftNumbers = Employees.Where(e => !EmployeesWithTimeOff.Contains(e)).Select(e => schedule.Shifts.Count(s => s.Employee == e)).ToList();

        var maxShiftsPerMonth = shiftNumbers.Max();
        var minShiftsPerMonth = shiftNumbers.Min();
        Assert.True(maxShiftsPerMonth - minShiftsPerMonth <= 2, $"Minimum number of shifts {minShiftsPerMonth} is too different from maximum {maxShiftsPerMonth}");
        
        var shiftNumbersForEmployeesWithTimeOff = EmployeesWithTimeOff.Select(e => schedule.Shifts.Count(s => s.Employee == e)).ToList();
        var maxShiftsTimeOffPerMonth = shiftNumbersForEmployeesWithTimeOff.Max();
        var minShiftsTimeOffPerMonth = shiftNumbersForEmployeesWithTimeOff.Min();
        Assert.True(maxShiftsTimeOffPerMonth - minShiftsTimeOffPerMonth <= 2, $"Minimum number of shifts {minShiftsTimeOffPerMonth} is too different from maximum {maxShiftsTimeOffPerMonth}");
        
        Assert.True(Math.Abs(2 * maxShiftsTimeOffPerMonth - maxShiftsPerMonth) <= 1, $"Employees with half of month off should have about half the number of total shifts, but had {maxShiftsTimeOffPerMonth} which is not half of {maxShiftsPerMonth}");
    }

    [Fact]
    public void AssignShiftsAccordingToPreferencePercentages()
    {
        var schedule = ComputeSchedule();

        var percentualShifts = schedule.Shifts.Where(s => s.Employee == _percentualCapabilitiesEmployee1.Employee).ToArray();
        var totalShiftCount = percentualShifts.Length;

        var shiftsOnAro1 = percentualShifts.Count(s => s.Shift.Workplace == Aro1);
        var shiftsOnAro2 = percentualShifts.Count(s => s.Shift.Workplace == Aro2);
        var shiftsOnAro3 = percentualShifts.Count(s => s.Shift.Workplace == Aro3);
        var shiftsOnRes1 = percentualShifts.Count(s => s.Shift.Workplace == Res1);
        
        Assert.Equal(totalShiftCount, shiftsOnAro1 + shiftsOnAro2 + shiftsOnAro3 + shiftsOnRes1);
        Assert.True(Math.Abs(totalShiftCount * 0.2 - shiftsOnAro1) <= 1, $"Shifts on the workplace should be about 20 % of total shifts ({totalShiftCount}) but is {shiftsOnAro1}");
        Assert.True(Math.Abs(totalShiftCount * 0.2 - shiftsOnAro2) <= 1, $"Shifts on the workplace should be about 20 % of total shifts ({totalShiftCount}) but is {shiftsOnAro2}");
        Assert.True(Math.Abs(totalShiftCount * 0.4 - shiftsOnAro3) <= 1, $"Shifts on the workplace should be about 40 % of total shifts ({totalShiftCount}) but is {shiftsOnAro3}");
        Assert.True(Math.Abs(totalShiftCount * 0.2 - shiftsOnRes1) <= 1, $"Shifts on the workplace should be about 20 % of total shifts ({totalShiftCount}) but is {shiftsOnRes1}");
    }

    [Fact]
    public void AssignShiftsAccordingToPreferencePercentagesWithLeave()
    {
        var schedule = ComputeSchedule();

        var percentualShifts = schedule.Shifts.Where(s => s.Employee == _percentualCapabilitiesEmployee2WithLeave.Employee).ToArray();
        var totalShiftCount = percentualShifts.Length;

        var shiftsOnAro3 = percentualShifts.Count(s => s.Shift.Workplace == Aro3);
        var shiftsOnRes3 = percentualShifts.Count(s => s.Shift.Workplace == Res3);
        
        Assert.Equal(totalShiftCount, shiftsOnAro3 + shiftsOnRes3);
        Assert.True(Math.Abs(totalShiftCount * 0.8 - shiftsOnAro3) <= 1, $"Shifts on the workplace should be about 80 % of total shifts ({totalShiftCount}) but is {shiftsOnAro3}");
        Assert.True(Math.Abs(totalShiftCount * 0.2 - shiftsOnRes3) <= 1, $"Shifts on the workplace should be about 20 % of total shifts ({totalShiftCount}) but is {shiftsOnRes3}");
    }

    [Fact]
    public void NotAssignShiftsToEmployeesOnLeave()
    {
        var schedule = ComputeSchedule();

        foreach (var employeeMonthWithTimeOff in EmployeeMonthsWithTimeOff)
        {
            var leave = employeeMonthWithTimeOff.Leaves.First() ?? throw new NullReferenceException();

            var assignedShifts = schedule.Shifts
                .Where(s => s.Employee == employeeMonthWithTimeOff.Employee)
                .Where(s => leave.From <= s.Shift.Day && s.Shift.Day <= leave.To)
                .ToList();
            
            Assert.Empty(assignedShifts);
        }
    }

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void HaveAtMostOneOfEachWeekendDaysAssigned(DayOfWeek weekendDay)
    {
        var schedule = ComputeSchedule();

        foreach (var employee in Employees)
        {
            var weekendDayShifts = schedule.Shifts
                .Where(s => s.Employee == employee && s.Shift.Day.DayOfWeek == weekendDay)
                .ToArray();
            
            Assert.True(weekendDayShifts.Length <= 1, $"More than one shift for {employee.Name} on {weekendDay.ToString()}: {string.Join(", ", weekendDayShifts.Select(s => s.Shift.Day))}");
        }
    }

    [Fact]
    public void SpreadAssignedShiftsEvenlyAcrossMonth()
    {
        var schedule = ComputeSchedule();

        foreach (var employee in EmployeesWithoutTimeOff)
        {
            var employeeShifts = schedule.Shifts.Where(s => s.Employee == employee);

            var boundaries = new [] { 1, 8, 15, 22, 29 };
            var shiftCounts = boundaries.Take(boundaries.Length - 1)
                .Zip(boundaries.Skip(1), (dayFrom, dayTo) => employeeShifts.Count(s => dayFrom <= s.Shift.Day.Date.Day && s.Shift.Day.Date.Day < dayTo))
                .ToArray();

            Assert.True(shiftCounts.Max() - shiftCounts.Min() < 3, $"Too large differences between shift counts for {employee.Name}: {string.Join(", ", shiftCounts)}");
        }
    }

    [Fact]
    public void SpreadAssignedShiftsEvenlyAcrossMonthTakingLeavesIntoAccount()
    {
        var scheduler = new ShiftScheduler();

        var week1start = 1;
        var week2start = 8;
        var week3start = 15;
        var week4start = 22;

        var boundaries = new[] {week1start, week2start, week3start, week4start, 29};

        var oddWeekLeaves = new []
        {
            new Leave(new Day(2022, 2, week1start), new Day(2022, 2, week1start + 3)), 
            new Leave(new Day(2022, 2, week3start), new Day(2022, 2, week3start + 3))
        };
        var evenWeekLeaves = new []
        {
            new Leave(new Day(2022, 2, week2start), new Day(2022, 2, week2start + 3)), 
            new Leave(new Day(2022, 2, week4start), new Day(2022, 2, week4start + 3))
        };

        var oddWeekLeaveEmployees = Enumerable.Range(1,3).Select(i => 
                new EmployeeMonth(
                new Employee($"Employee{i}", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Aro1, 100)}), 999),
                oddWeekLeaves))
            .ToArray();

        var evenWeekLeaveEmployees = Enumerable.Range(4,3).Select(i => 
                new EmployeeMonth(
                new Employee($"Employee{i}", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Aro1, 100)}), 999),
                evenWeekLeaves))
            .ToArray();

        var result = scheduler.ComputeSchedule(
            new MonthScheduleParameters(
                new ScheduledMonth(2022, 2),
                oddWeekLeaveEmployees.Concat(evenWeekLeaveEmployees).ToArray(),
                new [] {new CompetenceGroup(new [] {Aro1})}
            ));
        var successResult = Assert.IsType<SuccessfulSchedulingResult>(result);
        Assert.Equal(successResult.Flexibility, ScheduleFlexibility.Strict);
        var schedule = successResult.Schedule;
        
        foreach (var employeeMonth in oddWeekLeaveEmployees)
        {
            var employeeShifts = schedule.Shifts.Where(s => s.Employee == employeeMonth.Employee);
        
            var shiftCounts = boundaries.Take(boundaries.Length - 1)
                .Zip(boundaries.Skip(1), (dayFrom, dayTo) => employeeShifts.Count(s => dayFrom <= s.Shift.Day.Date.Day && s.Shift.Day.Date.Day < dayTo))
                .ToArray();

            var calibratedShiftCounts = shiftCounts.Zip(new[] {2, 1, 2, 1}, (int x, int y) => x * y).ToArray();
        
            Assert.True(calibratedShiftCounts.Max() - calibratedShiftCounts.Min() < 3, $"Too large differences between shift counts for {employeeMonth.Employee.Name}: {string.Join(", ", shiftCounts)}");
        }
    }
    
    [Fact]
    public void AlwaysScheduleWeekendsWithAlternates()
    {
        var schedule = ComputeSchedule();

        Assert.All(
            Employees, employee =>
            {
                var scheduledFridayShifts = schedule.Shifts
                    .Where(s => s.Employee == employee && s.Shift.Day.DayOfWeek == DayOfWeek.Friday)
                    .Select(s => s.Shift.Day.Date.Day)
                    .ToArray();

                var scheduledSundayShifts = schedule.Shifts
                    .Where(s => s.Employee == employee && s.Shift.Day.DayOfWeek == DayOfWeek.Sunday)
                    .Select(s => s.Shift.Day.Date.Day)
                    .ToArray();

                var expectedSundayShifts = scheduledFridayShifts
                    .Select(d => d + 2)
                    .Where(d => d <= 28)
                    .ToArray();

                Assert.Equal(expectedSundayShifts, scheduledSundayShifts);
            });
    }
    
    private Schedule ComputeSchedule()
    {
        var scheduler = new ShiftScheduler();

        var result = scheduler.ComputeSchedule(
            new MonthScheduleParameters(
                new ScheduledMonth(2022, 2),
                _employeeMonths,
                _competenceGroups
            ));
        var successResult = Assert.IsType<SuccessfulSchedulingResult>(result);
        Assert.True(successResult.Flexibility.Degree <= 1, $"Scheduler was forced to use too much flexibility (flexibility degree: {successResult.Flexibility.Degree}");
        return successResult.Schedule;
    }

    private static int DaysBetween(DateOnly d1, DateOnly d2) =>
        d2.ToDateTime(new TimeOnly(0, 0)).Subtract(d1.ToDateTime(new TimeOnly(0, 0))).Days;
}
