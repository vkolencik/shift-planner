using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using ShiftPlanner.Rules;
using Xunit;
using static ShiftPlanner.Engine.Tests.RulesFixture;

namespace ShiftPlanner.Engine.Tests;

public class RulesShould
{
    private readonly Mock<ISchedulingEngine> _engineMock;

    private static readonly ScheduledMonth _scheduledMonth = new ScheduledMonth(2022, 2);

    private static readonly MonthScheduleParameters MonthScheduleParameters = new MonthScheduleParameters(
        _scheduledMonth,
        new[]
        {
            new EmployeeMonth(Employee1, new[] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 2))}),
            new EmployeeMonth(Employee2),
            new EmployeeMonth(Employee3)
        },
        new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})},
        Array.Empty<ShiftAssignment>(),
        Array.Empty<ShiftAssignment>(),
        Array.Empty<ISet<Workplace>>());

    private static IEnumerable<Workplace> Workplaces => new[] {Workplace1, Workplace2};

    private static readonly IEnumerable<int> Fridays = new[] {4, 11, 18, 25};
    private static readonly IEnumerable<int> Saturdays = new[] {5, 12, 19, 26};
    private static readonly IEnumerable<int> Sundays = new[] {6, 13, 20, 27};

    public RulesShould()
    {
        _engineMock = new Mock<ISchedulingEngine>();
    }

    [Fact]
    public void DisallowConcurrentShiftsForEmployee()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        Rules.EveryEmployeeHasOneShiftAtATime(SolverParametersTwoDays, _engineMock.Object);

        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new[] {new ShiftAssignment(Employee1, Day1, Workplace1), new ShiftAssignment(Employee1, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(Employee1, Day2, Workplace1), new ShiftAssignment(Employee1, Day2, Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(Employee2, Day1, Workplace1), new ShiftAssignment(Employee2, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(Employee2, Day2, Workplace1), new ShiftAssignment(Employee2, Day2, Workplace2)}, capturedShifts);
    }

    [Fact]
    public void DisallowConsecutiveShiftsForEmployee()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsZero = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsZero), Constraint.Eq, 0));

        var previousMonthDays = _scheduledMonth.Previous.Days;
        var parameters = MonthScheduleParameters with
        {
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, previousMonthDays.ElementAt(previousMonthDays.Count - 1), Workplace1),
            }
        };

        Rules.NoConsecutiveShiftsForEmployee(parameters, _engineMock.Object);

        Assert.Equal(parameters.Employees.Count * (_scheduledMonth.Days.Count - 1), capturedShifts.Count);
        var day1 = _scheduledMonth.Days.ElementAt(0);
        var day2 = _scheduledMonth.Days.ElementAt(1);

        // consecutive shift within month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, day1, Workplace1),
                new ShiftAssignment(Employee1, day1, Workplace2),
                new ShiftAssignment(Employee1, day2, Workplace1),
                new ShiftAssignment(Employee1, day2, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        // consecutive shift within month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee2, day1, Workplace1),
                new ShiftAssignment(Employee2, day1, Workplace2),
                new ShiftAssignment(Employee2, day2, Workplace1),
                new ShiftAssignment(Employee2, day2, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        // consecutive shift from last month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace1),
                new ShiftAssignment(Employee1, Day1, Workplace2),
            }, capturedShiftsZero.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Fact]
    public void DisallowShiftsOnAlternateDaysExceptWeekends()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        var day1 = _scheduledMonth.Days.ElementAt(0);
        var day3 = _scheduledMonth.Days.ElementAt(2);
        var firstFriday = _scheduledMonth.Days.First(d => d.DayOfWeek == DayOfWeek.Friday);
        var firstSundayAfterFriday = _scheduledMonth.Days.SkipWhile(d => d.DayOfWeek != DayOfWeek.Friday).First(d => d.DayOfWeek == DayOfWeek.Sunday);

        var monthScheduleParameters = MonthScheduleParameters;

        Rules.NoAlternativeDayShiftsForEmployee(monthScheduleParameters, _engineMock.Object);

        // alternating shift within month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, day1, Workplace1),
                new ShiftAssignment(Employee1, day1, Workplace2),
                new ShiftAssignment(Employee1, day3, Workplace1),
                new ShiftAssignment(Employee1, day3, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        // alternating shift within month:
        Assert.DoesNotContain(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, firstFriday, Workplace1),
                new ShiftAssignment(Employee1, firstFriday, Workplace2),
                new ShiftAssignment(Employee1, firstSundayAfterFriday, Workplace1),
                new ShiftAssignment(Employee1, firstSundayAfterFriday, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Fact]
    public void DisallowShiftsOnAlternateDaysExceptWeekends_FromLastMonth()
    {
        var capturedShiftsZero = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsZero), Constraint.Eq, 0));

        var scheduledMonth = new ScheduledMonth(2022, 1);
        var day1 = scheduledMonth.Days.ElementAt(0);
        var day2 = scheduledMonth.Days.ElementAt(1);
        var previousMonthDays = scheduledMonth.Previous.Days;

        var monthScheduleParameters = MonthScheduleParameters with
        {
            Month = scheduledMonth,
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, previousMonthDays.ElementAt(previousMonthDays.Count - 1), Workplace1), // Friday
                new ShiftAssignment(Employee2, previousMonthDays.ElementAt(previousMonthDays.Count - 2), Workplace1), // Thursday
            }
        };

        Rules.NoAlternativeDayShiftsForEmployee(monthScheduleParameters, _engineMock.Object);

        // alternating shifts from last month:
        Assert.DoesNotContain(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, day2, Workplace1), // Sunday
                new ShiftAssignment(Employee1, day2, Workplace2),
            }, capturedShiftsZero.Select(x => new HashSet<ShiftAssignment>(x)));

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee2, day1, Workplace1), // Saturday
                new ShiftAssignment(Employee2, day1, Workplace2),
            }, capturedShiftsZero.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Fact]
    public void EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Gte, 1));

        Rules.EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(MonthScheduleParameters, new[] {Day1, Day2}, _engineMock.Object);

        Assert.Equal(4, capturedShifts.Count);

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace1),
                new ShiftAssignment(Employee3, Day1, Workplace1)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace2),
                new ShiftAssignment(Employee3, Day1, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Fact]
    public void EmployeeShouldOnlyBeAssignedToWorkplacesWhereTheyHaveCapability()
    {
        var capturedShifts = new List<ShiftAssignment>();
        _engineMock.Setup(_ => _.AddVariableConstraint(Capture.In(capturedShifts), Constraint.Eq, 0));

        Rules.DisallowShiftsWithoutEmployeeCapability(SolverParametersTwoDays, _engineMock.Object);

        Assert.Contains(new ShiftAssignment(Employee1, Day1, Workplace2), capturedShifts);
        Assert.Contains(new ShiftAssignment(Employee1, Day2, Workplace2), capturedShifts);
        Assert.DoesNotContain(new ShiftAssignment(Employee2, Day1, Workplace1), capturedShifts);
        Assert.DoesNotContain(new ShiftAssignment(Employee2, Day1, Workplace2), capturedShifts);
    }

    [Fact]
    public void RespectDaysOff()
    {
        var capturedShifts = new List<ShiftAssignment>();
        _engineMock.Setup(_ => _.AddVariableConstraint(Capture.In(capturedShifts), Constraint.Eq, 0));

        Rules.TakeLeavesIntoAccount(MonthScheduleParameters, _engineMock.Object);

        Assert.Equal(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, new Day(2022, 2, 1), Workplace1),
                new ShiftAssignment(Employee1, new Day(2022, 2, 1), Workplace2),
                new ShiftAssignment(Employee1, new Day(2022, 2, 2), Workplace1),
                new ShiftAssignment(Employee1, new Day(2022, 2, 2), Workplace2)
            },
            capturedShifts.ToHashSet());
    }

    [Fact]
    public void LimitWeekendShifts()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        Rules.LimitWeekendShifts(MonthScheduleParameters, _engineMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                Assert.Contains(
                    Saturdays.SelectMany(d => Workplaces.Select(workplace => new ShiftAssignment(employee, new Day(2022, 2, d), workplace))).ToHashSet(),
                    capturedShifts.Select(_ => _.ToHashSet()));

                Assert.Contains(
                    Sundays.SelectMany(d => Workplaces.Select(workplace => new ShiftAssignment(employee, new Day(2022, 2, d), workplace))).ToHashSet(),
                    capturedShifts.Select(_ => _.ToHashSet()));
            });
    }

    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends()
    {
        var capturedShiftsLhs = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsRhs = new List<IReadOnlyCollection<ShiftAssignment>>();

        _engineMock.Setup(_ => _.AddSumRelationConstraint(Capture.In(capturedShiftsLhs), Constraint.Eq, Capture.In(capturedShiftsRhs)));

        Rules.AlwaysScheduleAlternatesOverWeekends(MonthScheduleParameters, _engineMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                Assert.All(
                    Workplaces, workplace =>
                    {
                        Assert.All<(int, int)>(
                            Fridays.Zip(Sundays), tuple =>
                            {
                                var (friday, sunday) = tuple;
                                Assert.Contains(
                                    new HashSet<ShiftAssignment> {new ShiftAssignment(employee, new Day(_scheduledMonth, friday), workplace)},
                                    capturedShiftsLhs.Select(_ => _.ToHashSet()));
                                Assert.Contains(
                                    new HashSet<ShiftAssignment> {new ShiftAssignment(employee, new Day(_scheduledMonth, sunday), workplace)},
                                    capturedShiftsRhs.Select(_ => _.ToHashSet()));
                            });
                    });
            });
    }

    [Fact]
    public void NotScheduleSubsequentWeekends()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        Rules.AvoidSubsequentWeekends(MonthScheduleParameters, _engineMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                for (var i = 0; i < Saturdays.Count() - 1; i++)
                {
                    var expectedShifts = Workplaces
                        .Select(w => new ShiftAssignment(employee, new Day(_scheduledMonth, Saturdays.ElementAt(i)), w))
                        .Concat(Workplaces.Select(w => new ShiftAssignment(employee, new Day(_scheduledMonth, Sundays.ElementAt(i)), w)))
                        .Concat(Workplaces.Select(w => new ShiftAssignment(employee, new Day(_scheduledMonth, Saturdays.ElementAt(i + 1)), w)))
                        .Concat(Workplaces.Select(w => new ShiftAssignment(employee, new Day(_scheduledMonth, Sundays.ElementAt(i + 1)), w)))
                        .ToHashSet();
                    Assert.Contains(expectedShifts, capturedShifts.Select(_ => _.ToHashSet()));
                }
            });
    }

    [Fact]
    public void NotScheduleSubsequentWeekends_FromLastMonth()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Eq, 0));

        var parameters = MonthScheduleParameters with
        {
            Month = new ScheduledMonth(2023, 1),
            CompetenceGroups = new [] { new CompetenceGroup(new [] {Workplace1}) },
            LastMonthShifts = new []
            {
                new ShiftAssignment(Employee1, new Day(2022, 12, 31), Workplace1), 
                new ShiftAssignment(Employee2, new Day(2022, 12, 25), Workplace1), 
                new ShiftAssignment(Employee3, new Day(2022, 12, 24), Workplace1), 
            }
        };
        
        Rules.AvoidSubsequentWeekends(parameters, _engineMock.Object);

        Assert.Contains(new HashSet<ShiftAssignment>
        {
            new ShiftAssignment(Employee1, new Day(2023, 1, 7), Workplace1),
            new ShiftAssignment(Employee1, new Day(2023, 1, 8), Workplace1)
        }.ToHashSet(), capturedShifts.Select(_ => _.ToHashSet()));

        Assert.Contains(new HashSet<ShiftAssignment>
        {
            new ShiftAssignment(Employee2, new Day(2023, 1, 1), Workplace1),
        }.ToHashSet(), capturedShifts.Select(_ => _.ToHashSet()));
        
        Assert.Contains(new HashSet<ShiftAssignment>
        {
            new ShiftAssignment(Employee3, new Day(2023, 1, 1), Workplace1),
        }.ToHashSet(), capturedShifts.Select(_ => _.ToHashSet()));
    }
    
    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends_WithInterchangeability()
    {
        var capturedShiftsLhs = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsRhs = new List<IReadOnlyCollection<ShiftAssignment>>();

        _engineMock.Setup(_ => _.AddSumRelationConstraint(Capture.In(capturedShiftsLhs), Constraint.Eq, Capture.In(capturedShiftsRhs)));

        var fridays = _scheduledMonth.Days.Where(d => d.DayOfWeek == DayOfWeek.Friday).Select(d => d.DayOfMonth);
        var sundays = _scheduledMonth.Days.SkipWhile(d => d.DayOfWeek != DayOfWeek.Friday).Where(d => d.DayOfWeek == DayOfWeek.Sunday).Select(d => d.DayOfMonth);

        var monthScheduleParameters = MonthScheduleParameters with
        {
            WeekendInterchangeabilityGroups = new[]
            {
                new HashSet<Workplace> {Workplace1, Workplace2}
            }
        };

        Rules.AlwaysScheduleAlternatesOverWeekends(monthScheduleParameters, _engineMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                Assert.All(
                    new[] {Workplace1, Workplace2}, workplace =>
                    {
                        Assert.All<(int, int)>(
                            fridays.Zip(sundays), tuple =>
                            {
                                var (friday, sunday) = tuple;
                                Assert.Contains(
                                    new HashSet<ShiftAssignment>
                                    {
                                        new ShiftAssignment(employee, new Day(_scheduledMonth, friday), Workplace1),
                                        new ShiftAssignment(employee, new Day(_scheduledMonth, friday), Workplace2),
                                    },
                                    capturedShiftsLhs.Select(_ => _.ToHashSet()));
                                Assert.Contains(
                                    new HashSet<ShiftAssignment>
                                    {
                                        new ShiftAssignment(employee, new Day(_scheduledMonth, sunday), Workplace1),
                                        new ShiftAssignment(employee, new Day(_scheduledMonth, sunday), Workplace2),
                                    },
                                    capturedShiftsRhs.Select(_ => _.ToHashSet()));
                            });
                    });
            });
    }

    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends_FromLastMonth()
    {
        var capturedShiftsVariable = new List<IReadOnlyCollection<ShiftAssignment>>();

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsVariable), Constraint.Eq, 1));

        var month = new ScheduledMonth(2022, 1);

        var monthScheduleParameters = MonthScheduleParameters with
        {
            Month = month,
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, new Day(2021, 12, 31), Workplace1), // Friday 
            }
        };

        Rules.AlwaysScheduleAlternatesOverWeekends(monthScheduleParameters, _engineMock.Object);

        Assert.Equal(DayOfWeek.Friday, month.Previous.Days.Last().DayOfWeek);
        Assert.Contains(new List<ShiftAssignment> {new ShiftAssignment(Employee1, new Day(month, 2), Workplace1)}, capturedShiftsVariable);
    }

    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends_FromLastMonth_WithInterchangeability()
    {
        var capturedShiftsVariable = new List<IReadOnlyCollection<ShiftAssignment>>();

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsVariable), Constraint.Eq, 1));

        var month = new ScheduledMonth(2022, 1);

        var monthScheduleParameters = MonthScheduleParameters with
        {
            Month = month,
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, new Day(2021, 12, 31), Workplace1), // Friday 
            },
            WeekendInterchangeabilityGroups = new[]
            {
                new HashSet<Workplace> {Workplace1, Workplace2}
            }
        };

        Rules.AlwaysScheduleAlternatesOverWeekends(monthScheduleParameters, _engineMock.Object);

        Assert.Equal(DayOfWeek.Friday, month.Previous.Days.Last().DayOfWeek);
        Assert.Contains(
            new List<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, new Day(month, 2), Workplace1),
                new ShiftAssignment(Employee1, new Day(month, 2), Workplace2),
            }, capturedShiftsVariable);
    }

    [Fact]
    public void EnforceFixedShifts()
    {
        var capturedShifts = new List<ShiftAssignment>();

        _engineMock.Setup(_ => _.AddVariableConstraint(Capture.In(capturedShifts), Constraint.Eq, 1));

        var shiftAssignment1 = new ShiftAssignment(Employee1, new Day(2022, 2, 12), Workplace1);
        var shiftAssignment2 = new ShiftAssignment(Employee2, new Day(2022, 2, 27), Workplace2);
        var schedulingParams = MonthScheduleParameters with {FixedShifts = new[] {shiftAssignment1, shiftAssignment2}};

        Rules.EnforceFixedShifts(schedulingParams, _engineMock.Object);

        Assert.Contains(shiftAssignment1, capturedShifts);
        Assert.Contains(shiftAssignment2, capturedShifts);
    }
}
