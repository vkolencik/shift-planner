using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class MinNumberOfWeekendShiftsRuleShould
{
    private readonly Mock<IScheduleConstraintReceiver> _receiverMock = new Mock<IScheduleConstraintReceiver>();

    [Fact]
    public void EnforceMinNumberOfWeekendShifts()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, 1));

        var schedulingParams = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(Employee1),
                new EmployeeMonth(Employee2, new[] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 28))})
            },
            new[] {new CompetenceGroup(new[] {Workplace1})},
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>());

        var rule = new MinNumberOfWeekendShiftsRule(schedulingParams);
        rule.Apply(_receiverMock.Object);

        var capturedShifts = Assert.Single(capturedShiftsGte);
        Assert.All(capturedShifts, s => Assert.Equal(Employee1, s.Employee));
        var weekendDays = new HashSet<Day>{
            new Day(2022, 2, 5),
            new Day(2022, 2, 6),
            new Day(2022, 2, 12),
            new Day(2022, 2, 13),
            new Day(2022, 2, 19),
            new Day(2022, 2, 20),
            new Day(2022, 2, 26),
            new Day(2022, 2, 27)};
        Assert.Equal(weekendDays, capturedShifts.Select(s => s.Shift.Day).ToHashSet());
    }
}
