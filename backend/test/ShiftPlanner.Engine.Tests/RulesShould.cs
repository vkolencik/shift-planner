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
    private static readonly Workplace Workplace1 = new Workplace("Workplace1");
    private static readonly Workplace Workplace2 = new Workplace("Workplace2");
    private static readonly Capabilities CapabilitiesFirstWorkplace = new Capabilities(new WorkplaceCapability[] { new WorkplaceCapability.Green(Workplace1, 100), new WorkplaceCapability.Red(Workplace2) });
    private static readonly Capabilities CapabilitiesBothWorkplaces = new Capabilities(new WorkplaceCapability[] { new WorkplaceCapability.Yellow(Workplace1), new WorkplaceCapability.Green(Workplace2, 100) });
    private static readonly Employee Employee1 = new Employee("Employee1", CompetenceLevel.Level1, CapabilitiesFirstWorkplace);
    private static readonly Employee Employee2 = new Employee("Employee2", CompetenceLevel.Level2, CapabilitiesBothWorkplaces);
    private static readonly Employee Employee3 = new Employee("Employee3", CompetenceLevel.Level1, CapabilitiesBothWorkplaces);

    private static readonly SchedulingParameters SchedulingParameters = new SchedulingParameters(
        new ScheduledMonth(2022, 2),
        new[] {Employee1, Employee2, Employee3},
        new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})});
    
    private static readonly SolverParameters SolverParametersTwoDays = new SolverParameters(
        new[] {Day1, Day2},
        new[] {Employee1, Employee2},
        new[] {Workplace1, Workplace2});

    private static readonly SolverParameters SolverParametersOneWeek = new SolverParameters(
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

        Rules.EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(SchedulingParameters, new [] {Day1, Day2}, _engineMock.Object);
        
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
    public void SpreadShiftAssignmentEvenly()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var solverParams = SolverParametersTwoDays;
        var standard = solverParams.Days.Count * solverParams.Workplaces.Count / solverParams.Employees.Count;
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, standard - 1));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, standard + 1));

        Rules.SpreadShiftAssignmentEvenly(solverParams, _engineMock.Object);
        
        
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace1),
                new ShiftAssignment(Employee1, Day2, Workplace1),
                new ShiftAssignment(Employee1, Day1, Workplace2),
                new ShiftAssignment(Employee1, Day2, Workplace2),
            }, capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x)));
    }
}
