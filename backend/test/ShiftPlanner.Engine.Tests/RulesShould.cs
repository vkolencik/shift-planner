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
