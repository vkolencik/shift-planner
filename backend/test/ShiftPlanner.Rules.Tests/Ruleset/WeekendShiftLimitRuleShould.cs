using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;
using static RulesFixture;

public class WeekendShiftLimitRuleShould
{
    [Fact]
    public void LimitWeekendShifts()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();

        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        var rule = new WeekendShiftLimitRule(MonthScheduleParameters);
        rule.Apply(receiverMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                Assert.Contains(
                    Saturdays.SelectMany(d => Workplaces.Select(workplace => new ShiftAssignment(employee, new Day(2022, 2, d), workplace))).ToHashSet(),
                    capturedShifts.Select(_ => _.ToHashSet()));

                Assert.Contains(
                    Sundays.SelectMany(d => Workplaces.Select(workplace => new ShiftAssignment(employee, new Day(2022, 2, d), workplace))).ToHashSet(),
                    capturedShifts.Select(_ => _.ToHashSet()));
            });
    }

}
