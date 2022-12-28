using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;
using static ShiftPlanner.Rules.Tests.RulesFixture;

public class LeaveRuleShould
{
    [Fact]
    public void RespectDaysOff()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();
        var capturedShifts = new List<ShiftAssignment>();
        receiverMock.Setup(_ => _.AddVariableConstraint(Capture.In(capturedShifts), Constraint.Eq, 0));

        var rule = new LeaveRule(MonthScheduleParameters);
        rule.Apply(receiverMock.Object);

        Assert.Equal(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, new Day(2022, 2, 1), Workplace1),
                new ShiftAssignment(Employee1, new Day(2022, 2, 1), Workplace2),
                new ShiftAssignment(Employee1, new Day(2022, 2, 2), Workplace1),
                new ShiftAssignment(Employee1, new Day(2022, 2, 2), Workplace2)
            },
            capturedShifts.ToHashSet());
    }

}
