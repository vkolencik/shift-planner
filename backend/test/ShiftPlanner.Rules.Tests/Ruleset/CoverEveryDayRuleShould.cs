using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

public class CoverEveryDayRuleShould
{
    [Fact]
    public void CoverEveryDayAtEveryWorkplace()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();

        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Eq, 1));

        var rule = new EveryDayAtEveryWorkplaceIsCoveredRule(RulesFixture.SolverParametersTwoDays);
        rule.Apply(receiverMock.Object);

        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day1, RulesFixture.Workplace1), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day1, RulesFixture.Workplace1)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day1, RulesFixture.Workplace2), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day1, RulesFixture.Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day2, RulesFixture.Workplace1), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day2, RulesFixture.Workplace1)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day2, RulesFixture.Workplace2), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day2, RulesFixture.Workplace2)}, capturedShifts);
    }
}
