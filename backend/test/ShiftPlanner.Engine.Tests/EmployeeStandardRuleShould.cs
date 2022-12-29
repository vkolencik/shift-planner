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

public class EmployeeStandardRuleShould
{
    private readonly Mock<ISchedulingEngine> _engineMock;
    private readonly Mock<ISchedulingParametersRecorder> _recorderMock = new Mock<ISchedulingParametersRecorder>();

    public EmployeeStandardRuleShould()
    {
        _engineMock = new Mock<ISchedulingEngine>();
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
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>(),
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
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>(),
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
