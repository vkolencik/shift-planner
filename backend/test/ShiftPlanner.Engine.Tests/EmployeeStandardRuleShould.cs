using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using Xunit;
using static ShiftPlanner.Engine.Tests.RulesFixture;

namespace ShiftPlanner.Engine.Tests;

public class EmployeeStandardRuleShould
{
    private readonly Mock<ISchedulingEngine> _engineMock;

    public EmployeeStandardRuleShould()
    {
        _engineMock = new Mock<ISchedulingEngine>();
    }

    [Fact]
    public void SpreadShiftAssignmentEvenlyAmongEmployees()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var schedulingParams = new SchedulerParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(Employee1),
                new EmployeeMonth(Employee2)
            },
            new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})},
            ScheduleFlexibility.Strict);

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count / schedulingParams.Employees.Count;

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (uint) Math.Floor(standard)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (uint) Math.Ceiling(standard)));

        EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulingParams, _engineMock.Object);

        Assert.Equal(capturedShiftsGte.ToHashSet(), capturedShiftsLte.ToHashSet());
        Assert.All(
            schedulingParams.Employees,
            employee => Assert.Contains(
                schedulingParams.Month.Days.SelectMany(d => new [] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(employee, d, w))).ToHashSet(),
                capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x))));
    }

    [Fact]
    public void SpreadShiftAssignmentEvenlyAmongEmployeesTakingLeavesIntoAccount()
    {
        var capturedShiftsFullMonthGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsFullMonthLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsHalfMonthOffGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsHalfMonthOffLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var schedulingParams = new SchedulerParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(Employee1, new [] { new Leave(new Day(2022, 2, 1), new Day(2022, 2, 14)) }),
                new EmployeeMonth(Employee2),
                new EmployeeMonth(Employee2),
            },
            new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})},
            ScheduleFlexibility.Strict);

        var v = (double) 2 * 28;
        var ceNoLeaves = (double) 28;
        var ceLeave = (double) 14;
        var ct = (double) 2.5 * 28;
        var standardNoLeaves = v * ceNoLeaves / ct;
        var standardLeave = v * ceLeave / ct;

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsFullMonthGte), Constraint.Gte, (uint) Math.Floor(standardNoLeaves)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsFullMonthLte), Constraint.Lte, (uint) Math.Ceiling(standardNoLeaves)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsHalfMonthOffGte), Constraint.Gte, (uint) Math.Floor(standardLeave)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsHalfMonthOffLte), Constraint.Lte, (uint) Math.Ceiling(standardLeave)));

        EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulingParams, _engineMock.Object);

        Assert.Equal(capturedShiftsFullMonthGte.ToHashSet(), capturedShiftsFullMonthLte.ToHashSet());
        Assert.Equal(capturedShiftsHalfMonthOffGte.ToHashSet(), capturedShiftsHalfMonthOffLte.ToHashSet());

        Assert.Contains(
            schedulingParams.Month.Days
                .SelectMany(d => new[] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(Employee1, d, w)))
                .ToHashSet(),
            capturedShiftsHalfMonthOffLte.Select(_ => _.ToHashSet()));
        
        Assert.Contains(
            schedulingParams.Month.Days
                .SelectMany(d => new[] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(Employee2, d, w)))
                .ToHashSet(),
            capturedShiftsFullMonthLte.Select(_ => _.ToHashSet()));
    }

    [Fact]
    public void AssignShiftsToEmployeesAccordingToPreferencePercentages()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new [] { new WorkplaceCapability.Green(Workplace1, 20), new WorkplaceCapability.Green(Workplace2, 80) }), 999);
        var schedulingParams = new SchedulerParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(employee)
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1, Workplace2})
            },
            ScheduleFlexibility.Strict
        );

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count / schedulingParams.Employees.Count;

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (uint) Math.Floor(standard * 0.2)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (uint) Math.Ceiling(standard * 0.2)));

        EmployeeStandardRule.AssignShiftsByPreferencePercentages(schedulingParams, _engineMock.Object);

        Assert.Equal(capturedShiftsGte.ToHashSet(), capturedShiftsLte.ToHashSet()); 
        Assert.Contains(
                schedulingParams.Month.Days.Select(d => new ShiftAssignment(employee, d, Workplace1)).ToHashSet(),
                capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Fact]
    public void TakeEmployeeMaxStandardIntoAccount()
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new [] { new WorkplaceCapability.Green(Workplace1, 100) }), 13);
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedBoundsLte = new List<uint>();
        
        var schedulingParams = new SchedulerParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(employee)
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            ScheduleFlexibility.Strict
        );

        _engineMock.Setup(_ => _.AddSumConstraint(It.IsAny<IReadOnlyCollection<ShiftAssignment>>(), Constraint.Lte, Capture.In(capturedBoundsLte)));

        EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulingParams, _engineMock.Object);

        var lteBound = Assert.Single(capturedBoundsLte);
        Assert.Equal(employee.MaxShiftsPerMonth, lteBound);
    }

    [Fact]
    public void SpreadShiftsEvenlyThroughoutMonth()
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new [] { new WorkplaceCapability.Green(Workplace1, 100) }), 999);

        var schedulingParams = new SchedulerParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(employee)
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            ScheduleFlexibility.Strict
        );

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count;

        List<IReadOnlyCollection<ShiftAssignment>> capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        List<IReadOnlyCollection<ShiftAssignment>> capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (uint) Math.Floor(standard / 4)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (uint) Math.Ceiling(standard / 4)));

        EmployeeStandardRule.SpreadShiftsEvenlyThroughoutMonth(schedulingParams, _engineMock.Object);

        Assert.Equal(4, capturedShiftsLte.Count);

        Assert.Equal(capturedShiftsLte.Select(_ => _.ToHashSet()), capturedShiftsGte.Select(_ => _.ToHashSet()));

        var range1 = RangeFromTo(1, 7).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range2 = RangeFromTo(8, 14).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range3 = RangeFromTo(15, 21).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range4 = RangeFromTo(22, 28).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        Assert.Contains(range1, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range2, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range3, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range4, capturedShiftsLte.Select(_ => _.ToHashSet()));
    }

    private static IEnumerable<int> RangeFromTo(int from, int to) => Enumerable.Range(from, to - from + 1);
}
