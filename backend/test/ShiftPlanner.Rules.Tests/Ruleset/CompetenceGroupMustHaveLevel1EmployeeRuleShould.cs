using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class CompetenceGroupMustHaveLevel1EmployeeRuleShould
{
    [Fact]
    public void EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();

        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Gte, 1));

        var rule = new CompetenceGroupQualificationRule(MonthScheduleParameters);
        rule.Apply(receiverMock.Object);

        Assert.Equal(MonthScheduleParameters.Days.Count() * 2, capturedShifts.Count);

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace1),
                new ShiftAssignment(Employee3, Day1, Workplace1)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace2),
                new ShiftAssignment(Employee3, Day1, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));
    }
}
