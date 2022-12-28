using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class WorkplaceCompetenceRuleShould
{
    [Fact]
    public void OnlyBeAssignEmployeesToWorkplacesWhereTheyHaveCapability()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();

        var capturedShifts = new List<ShiftAssignment>();
        receiverMock.Setup(_ => _.AddVariableConstraint(Capture.In(capturedShifts), Constraint.Eq, 0));

        var rule = new WorkplaceCapabilityRule(SolverParametersTwoDays);
        rule.Apply(receiverMock.Object);

        Assert.Contains(new ShiftAssignment(Employee1, Day1, Workplace2), capturedShifts);
        Assert.Contains(new ShiftAssignment(Employee1, Day2, Workplace2), capturedShifts);
        Assert.DoesNotContain(new ShiftAssignment(Employee2, Day1, Workplace1), capturedShifts);
        Assert.DoesNotContain(new ShiftAssignment(Employee2, Day1, Workplace2), capturedShifts);
    }

}
