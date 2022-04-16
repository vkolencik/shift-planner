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
    private static readonly Employee Employee1 = new Employee("Employee1");
    private static readonly Employee Employee2 = new Employee("Employee2");
    private static readonly Workplace Workplace1 = new Workplace("Workplace1");
    private static readonly Workplace Workplace2 = new Workplace("Workplace2");

    private static readonly SolverParameters SchedulingParameters = new SolverParameters(
        new[] {Day1, Day2},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

    public RulesShould()
    {
        _engineMock = new Mock<ISchedulingEngine>();
    }

    /*
EveryEmployeeHasOneShiftAtATime
NoConsecutiveShiftsForEmployee
 */
    [Fact]
    public void CoverEveryDayAtEveryWorkplace()
    {
        var capturedShifts = new List<IReadOnlyCollection<Shift>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Eq, 1));

        Rules.EveryDayAtEveryWorkplaceIsCovered(SchedulingParameters, _engineMock.Object);

        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new [] { new Shift(Employee1, Day1, Workplace1), new Shift(Employee2, Day1, Workplace1)}, capturedShifts);
        Assert.Contains(new [] { new Shift(Employee1, Day1, Workplace2), new Shift(Employee2, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new Shift(Employee1, Day2, Workplace1), new Shift(Employee2, Day2, Workplace1)}, capturedShifts);
        Assert.Contains(new [] { new Shift(Employee1, Day2, Workplace2), new Shift(Employee2, Day2, Workplace2)}, capturedShifts);
    }

    [Fact]
    public void DisallowConcurrentShiftsForEmployee()
    {
        var capturedShifts = new List<IReadOnlyCollection<Shift>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lt, 1));
        
        Rules.EveryEmployeeHasOneShiftAtATime(SchedulingParameters, _engineMock.Object);
        
        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new [] { new Shift(Employee1, Day1, Workplace1), new Shift(Employee1, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new Shift(Employee1, Day2, Workplace1), new Shift(Employee1, Day2, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new Shift(Employee2, Day1, Workplace1), new Shift(Employee2, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new [] { new Shift(Employee2, Day2, Workplace1), new Shift(Employee2, Day2, Workplace2)}, capturedShifts);
    }

    [Fact]
    public void DisallowConsecutiveShiftsForEmployee()
    {
        var capturedShifts = new List<IReadOnlyCollection<Shift>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lt, 1));

        Rules.NoConsecutiveShiftsForEmployee(SchedulingParameters, _engineMock.Object);
        
        Assert.Equal(2, capturedShifts.Count);
        Assert.Contains(new HashSet<Shift> {
            new Shift(Employee1, Day1, Workplace1),
            new Shift(Employee1, Day1, Workplace2),
            new Shift(Employee1, Day2, Workplace1),
            new Shift(Employee1, Day2, Workplace2)
        }, capturedShifts.Select(x => new HashSet<Shift>(x)));

        Assert.Contains(new HashSet<Shift> {
            new Shift(Employee2, Day1, Workplace1),
            new Shift(Employee2, Day1, Workplace2),
            new Shift(Employee2, Day2, Workplace1),
            new Shift(Employee2, Day2, Workplace2)
        }, capturedShifts.Select(x => new HashSet<Shift>(x)));
    }
}
