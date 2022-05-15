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
        var solverParams = SolverParametersTwoDays;
        
        var standard = solverParams.Days.Count * solverParams.Workplaces.Count / solverParams.Employees.Count;
        
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, standard));
        _engineMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, standard));

        EmployeeStandardRule.SpreadShiftAssignmentEvenly(solverParams, _engineMock.Object);

        Assert.All(solverParams.Employees, employee => Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(employee, Day1, Workplace1),
                new ShiftAssignment(employee, Day2, Workplace1),
                new ShiftAssignment(employee, Day1, Workplace2),
                new ShiftAssignment(employee, Day2, Workplace2),
            }, capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x))));
    }
}
