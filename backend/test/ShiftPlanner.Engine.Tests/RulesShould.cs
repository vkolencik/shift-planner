using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using Xunit;

namespace ShiftPlanner.Engine.Tests;

public class RulesShould
{
    private Mock<ISchedulingEngine> _engineMock;

    private static readonly Day Day1 = new Day(new DateOnly(2022, 2, 1));
    private static readonly Day Day2 = new Day(new DateOnly(2022, 2, 2));
    private static readonly Day Day3 = new Day(new DateOnly(2022, 2, 3));
    private static readonly Day Day4Friday = new Day(new DateOnly(2022, 2, 4));
    private static readonly Day Day5Saturday = new Day(new DateOnly(2022, 2, 5));
    private static readonly Day Day6Sunday = new Day(new DateOnly(2022, 2, 6));
    private static readonly Day Day7 = new Day(new DateOnly(2022, 2, 7));
    private static readonly Employee Employee1 = new Employee("Employee1");
    private static readonly Employee Employee2 = new Employee("Employee2");
    private static readonly Workplace Workplace1 = new Workplace("Workplace1");
    private static readonly Workplace Workplace2 = new Workplace("Workplace2");

    private static readonly SolverParameters SchedulingParametersTwoDays = new SolverParameters(
        new[] {Day1, Day2},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

    private static readonly SolverParameters SchedulingParametersOneWeek = new SolverParameters(
        new[] {Day1, Day2, Day3, Day4Friday, Day5Saturday, Day6Sunday, Day7},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

    public RulesShould()
    {
        _engineMock = new Mock<ISchedulingEngine>();
    }

    [Fact]
    public void CoverEveryDayAtEveryWorkplace()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Eq, 1));

        Rules.EveryDayAtEveryWorkplaceIsCovered(SchedulingParametersTwoDays, _engineMock.Object);

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
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lt, 1));
        
        Rules.EveryEmployeeHasOneShiftAtATime(SchedulingParametersTwoDays, _engineMock.Object);
        
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
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lt, 1));

        Rules.NoConsecutiveShiftsForEmployee(SchedulingParametersTwoDays, _engineMock.Object);
        
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
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lt, 1));

        Rules.NoAlternativeDayShiftsForEmployee(SchedulingParametersOneWeek, _engineMock.Object);

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
    
}
