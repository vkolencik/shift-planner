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
            Schedule.Empty,
            ScheduleFlexibility.Strict);

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count / schedulingParams.Employees.Count;

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (uint) Math.Floor(standard)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (uint) Math.Ceiling(standard)));

        EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulingParams, _engineMock.Object);

        Assert.Equal(capturedShiftsGte.ToHashSet(), capturedShiftsLte.ToHashSet());
        Assert.All(
            schedulingParams.Employees,
            employee => Assert.Contains(
                schedulingParams.Month.Days.SelectMany(d => new[] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(employee, d, w))).ToHashSet(),
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
                new EmployeeMonth(Employee1, new[] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 14))}),
                new EmployeeMonth(Employee2),
                new EmployeeMonth(Employee2),
            },
            new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})},
            Schedule.Empty,
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

        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 20), new WorkplaceCapability.Green(Workplace2, 80)}), Workload.Infinite);
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
            Schedule.Empty,
            ScheduleFlexibility.Strict
        );

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count / schedulingParams.Employees.Count;

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (uint) Math.Floor(standard * 0.2)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (uint) Math.Ceiling(standard * 0.2)));

        EmployeeStandardRule.AssignShiftsByPreferencePercentages(schedulingParams, _engineMock.Object);

        // TODO: 2022-12-05 min bounds relaxation
        // Assert.Equal(capturedShiftsGte.ToHashSet(), capturedShiftsLte.ToHashSet());
        Assert.Contains(
            schedulingParams.Month.Days.Select(d => new ShiftAssignment(employee, d, Workplace1)).ToHashSet(),
            capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x)));
    }

    [Theory]
    [MemberData(nameof(GetAllWorkloads))]
    public void TakeEmployeeMaxStandardIntoAccount(Workload workload)
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)}), workload);
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedBoundsLte = new List<uint>();

        var month = new ScheduledMonth(2022, 2);
        var schedulingParams = new SchedulerParameters(
            month,
            new[]
            {
                new EmployeeMonth(employee)
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            Schedule.Empty,
            ScheduleFlexibility.Strict
        );

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, Capture.In(capturedBoundsLte)));

        EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulingParams, _engineMock.Object);

        var lteBound = Assert.Single(capturedBoundsLte);
        var lteShifts = Assert.Single(capturedShiftsLte);

        Assert.Equal(workload.MaximumShiftsPerMonth, lteBound);
        Assert.Equal(month.Days.Select(d => new ShiftAssignment(employee, d, Workplace1)), lteShifts);
    }

    [Theory]
    [MemberData(nameof(GetAllWorkloads))]
    public void TakeEmployeeMaxStandardIntoAccountIncludingLeave(Workload workload)
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)}), workload);
        var capturedBoundsLte = new List<uint>();
        var capturedBoundsGte = new List<uint>();

        var month = new ScheduledMonth(2022, 2);
        var schedulingParams = new SchedulerParameters(
            month,
            new[]
            {
                new EmployeeMonth(employee, new[] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 14))})
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            Schedule.Empty,
            ScheduleFlexibility.Strict
        );

        _engineMock.Setup(_ => _.AddSumConstraint(It.IsAny<IReadOnlyCollection<ShiftAssignment>>(), Constraint.Lte, Capture.In(capturedBoundsLte)));
        _engineMock.Setup(_ => _.AddSumConstraint(It.IsAny<IReadOnlyCollection<ShiftAssignment>>(), Constraint.Gte, Capture.In(capturedBoundsGte)));

        EmployeeStandardRule.SpreadShiftAssignmentEvenlyAmongEmployees(schedulingParams, _engineMock.Object);

        var lteBound = Assert.Single(capturedBoundsLte);
        var gteBound = Assert.Single(capturedBoundsGte);

        Assert.Equal(Math.Ceiling((double) workload.MaximumShiftsPerMonth / 2), lteBound);

        // TODO: create a separate test for upper bound as well
        // Assert.Equal(Math.Floor((double) workload.MaximumShiftsPerMonth / 2), gteBound);
    }

    public static IEnumerable<object[]> GetAllWorkloads() => new[]
    {
        Workload.Workload04,
        Workload.Workload05,
        Workload.Workload06,
        Workload.Workload07,
        Workload.Workload08,
        Workload.Full
    }.Select(_ => new object[] {_});

    [Fact]
    public void SpreadShiftsEvenlyThroughoutMonth()
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)}), Workload.Infinite);

        var leave = new Leave(new Day(2022, 2, 1), new Day(2022, 2, 3));
        var schedulingParams = new SchedulerParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(
                    employee,
                    new[] {leave})
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            Schedule.Empty,
            ScheduleFlexibility.Strict
        );

        List<IReadOnlyCollection<ShiftAssignment>> capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        List<IReadOnlyCollection<ShiftAssignment>> capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedLowerBounds = new List<uint>();
        var capturedUpperBounds = new List<uint>();
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, Capture.In(capturedUpperBounds)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, Capture.In(capturedLowerBounds)));

        EmployeeStandardRule.SpreadShiftsEvenlyThroughoutMonth(schedulingParams, _engineMock.Object);

        Assert.Equal(4, capturedShiftsLte.Count);

        // TODO: 2022-12-05 min bounds relaxation
        // Assert.Equal(capturedShiftsLte.Select(_ => _.ToHashSet()), capturedShiftsGte.Select(_ => _.ToHashSet()));

        var range1 = RangeFromTo(1, 7).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range2 = RangeFromTo(8, 14).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range3 = RangeFromTo(15, 21).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range4 = RangeFromTo(22, 28).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        Assert.Contains(range1, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range2, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range3, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range4, capturedShiftsLte.Select(_ => _.ToHashSet()));

        // TODO: 2022-12-05 min bounds relaxation
        // Assert.All(capturedLowerBounds.Skip(1), lb => Assert.Equal(7U, lb));
        Assert.All(capturedUpperBounds.Skip(1), ub => Assert.Equal(8U, ub));
        // Three-day leave in the first week => lower standard:
        // TODO: 2022-12-05 min bounds relaxation
        // Assert.Equal(4U, capturedLowerBounds.First());
        Assert.Equal(5U, capturedUpperBounds.First());
    }

    private static IEnumerable<int> RangeFromTo(int from, int to) => Enumerable.Range(from, to - from + 1);
}
