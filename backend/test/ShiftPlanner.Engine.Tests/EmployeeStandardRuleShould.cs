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
    public void SpreadShiftAssignmentEvenly()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var schedulingParams = new SchedulingParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(Employee1),
                new EmployeeMonth(Employee2),
                new EmployeeMonth(Employee3)
            },
            new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})});

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count / schedulingParams.Employees.Count;

        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (int) Math.Floor(standard)));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (int) Math.Ceiling(standard)));

        EmployeeStandardRule.SpreadShiftAssignmentEvenly(schedulingParams, _engineMock.Object);

        Assert.Equal(capturedShiftsGte.ToHashSet(), capturedShiftsLte.ToHashSet());
        Assert.All(
            schedulingParams.Employees,
            employee => Assert.Contains(
                schedulingParams.Month.Days.SelectMany(d => new [] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(employee, d, w))).ToHashSet(),
                capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x))));
    }
}
