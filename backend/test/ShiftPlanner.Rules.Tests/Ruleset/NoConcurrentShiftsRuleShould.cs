using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class NoConcurrentShiftsRuleShould
{
    [Fact]
    public void DisallowConcurrentShiftsForEmployee()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        var rule = new NoConcurrentShiftsRule(SolverParametersTwoDays);
        rule.Apply(receiverMock.Object);

        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new[] {new ShiftAssignment(Employee1, Day1, Workplace1), new ShiftAssignment(Employee1, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(Employee1, Day2, Workplace1), new ShiftAssignment(Employee1, Day2, Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(Employee2, Day1, Workplace1), new ShiftAssignment(Employee2, Day1, Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(Employee2, Day2, Workplace1), new ShiftAssignment(Employee2, Day2, Workplace2)}, capturedShifts);
    }

}
