using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using Xunit;
using static ShiftPlanner.Engine.Tests.RulesFixture;

namespace ShiftPlanner.Engine.Tests;

public class RulesShould
{
    private readonly Mock<ISchedulingEngine> _engineMock;

    private static readonly MonthScheduleParameters MonthScheduleParameters = new MonthScheduleParameters(
        new ScheduledMonth(2022, 2),
        new[]
        {
            new EmployeeMonth(Employee1, new [] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 2))}),
            new EmployeeMonth(Employee2),
            new EmployeeMonth(Employee3)
        },
        new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})});

    private static IEnumerable<Workplace> Workplaces => new[] {Workplace1, Workplace2};

    private static IEnumerable<int> _fridays = new[] {4, 11, 18, 25};
    private static IEnumerable<int> _saturdays = new[] {5, 12, 19, 26};
    private static IEnumerable<int> _sundays = new[] {6, 13, 20, 27};

    public RulesShould()
    {
        _engineMock = new Mock<ISchedulingEngine>();
    }

    [Fact]
    public void CoverEveryDayAtEveryWorkplace()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Eq, 1));

        Rules.EveryDayAtEveryWorkplaceIsCovered(SolverParametersTwoDays, _engineMock.Object);

        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new [] { new ShiftAssignment(Employee1, Day1, Workplace1), new ShiftAssignment(Employee2, Day1, Workplace1)}, capturedShifts);
        Assert.Contains(new [] { new ShiftAssignment(Employee1, Day1, Workplace2), new ShiftAssignment(Employee2, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new ShiftAssignment(Employee1, Day2, Workplace1), new ShiftAssignment(Employee2, Day2, Workplace1)}, capturedShifts);
        Assert.Contains(new [] { new ShiftAssignment(Employee1, Day2, Workplace2), new ShiftAssignment(Employee2, Day2, Workplace2)}, capturedShifts);
    }

    [Fact]
    public void DisallowConcurrentShiftsForEmployee()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));
        
        Rules.EveryEmployeeHasOneShiftAtATime(SolverParametersTwoDays, _engineMock.Object);
        
        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new [] { new ShiftAssignment(Employee1, Day1, Workplace1), new ShiftAssignment(Employee1, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new ShiftAssignment(Employee1, Day2, Workplace1), new ShiftAssignment(Employee1, Day2, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new ShiftAssignment(Employee2, Day1, Workplace1), new ShiftAssignment(Employee2, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new ShiftAssignment(Employee2, Day2, Workplace1), new ShiftAssignment(Employee2, Day2, Workplace2)}, capturedShifts);
    }

    [Fact]
    public void DisallowConsecutiveShiftsForEmployee()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        Rules.NoConsecutiveShiftsForEmployee(SolverParametersTwoDays, _engineMock.Object);
        
        Assert.Equal(2, capturedShifts.Count);
        Assert.Contains(new HashSet<ShiftAssignment> {
            new ShiftAssignment(Employee1, Day1, Workplace1),
            new ShiftAssignment(Employee1, Day1, Workplace2),
            new ShiftAssignment(Employee1, Day2, Workplace1),
            new ShiftAssignment(Employee1, Day2, Workplace2)
        }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        Assert.Contains(new HashSet<ShiftAssignment> {
            new ShiftAssignment(Employee2, Day1, Workplace1),
            new ShiftAssignment(Employee2, Day1, Workplace2),
            new ShiftAssignment(Employee2, Day2, Workplace1),
            new ShiftAssignment(Employee2, Day2, Workplace2)
        }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Fact]
    public void DisallowShiftsOnAlternateDaysExceptWeekends()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        Rules.NoAlternativeDayShiftsForEmployee(SolverParametersOneWeek, _engineMock.Object);

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace1),
                new ShiftAssignment(Employee1, Day1, Workplace2),
                new ShiftAssignment(Employee1, Day3, Workplace1),
                new ShiftAssignment(Employee1, Day3, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        Assert.DoesNotContain(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day4Friday, Workplace1),
                new ShiftAssignment(Employee1, Day4Friday, Workplace2),
                new ShiftAssignment(Employee1, Day6Sunday, Workplace1),
                new ShiftAssignment(Employee1, Day6Sunday, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Fact]
    public void EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Gte, 1));

        Rules.EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(MonthScheduleParameters, new [] {Day1, Day2}, _engineMock.Object);
        
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
                    _saturdays.SelectMany(d => Workplaces.Select(workplace => new ShiftAssignment(employee, new Day(2022, 2, d), workplace))).ToHashSet(),
                    capturedShifts.Select(_ => _.ToHashSet()));

                Assert.Contains(
                    _sundays.SelectMany(d => Workplaces.Select(workplace => new ShiftAssignment(employee, new Day(2022, 2, d), workplace))).ToHashSet(),
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
                Assert.All<(int, int)>(_fridays.Zip(_sundays), tuple =>
                {
                    var (friday, sunday) = tuple;
                    Assert.Contains(
                        Workplaces.Select(w => new ShiftAssignment(employee, new Day(2022, 2, friday), w)),
                        capturedShiftsLhs.Select(_ => _.ToHashSet()));
                    Assert.Contains(
                        Workplaces.Select(w => new ShiftAssignment(employee, new Day(2022, 2, sunday), w)),
                        capturedShiftsRhs.Select(_ => _.ToHashSet()));
                });
            });
    }
}
