using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class PredefinedShiftsRuleShould
{
    [Fact]
    public void EnforcePredefinedShifts()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();

        var capturedShifts = new List<ShiftAssignment>();

        receiverMock.Setup(_ => _.AddVariableConstraint(Capture.In(capturedShifts), Constraint.Eq, 1));

        var shiftAssignment1 = new ShiftAssignment(Employee1, new Day(2022, 2, 12), Workplace1);
        var shiftAssignment2 = new ShiftAssignment(Employee2, new Day(2022, 2, 27), Workplace2);
        var schedulingParams = MonthScheduleParameters with {FixedShifts = new[] {shiftAssignment1, shiftAssignment2}};

        var rule = new PredefinedShiftsRule(schedulingParams);
        rule.Apply(receiverMock.Object);

        Assert.Contains(shiftAssignment1, capturedShifts);
        Assert.Contains(shiftAssignment2, capturedShifts);
    }
    
}
