using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class NoSubsequentWeekendRuleShould
{
    private readonly Mock<IScheduleConstraintReceiver> _receiverMock = new Mock<IScheduleConstraintReceiver>();

    [Fact]
    public void NotScheduleSubsequentWeekends()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        var rule = new NoSubsequentWeekendsRule(MonthScheduleParameters);
        rule.Apply(_receiverMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                for (var i = 0; i < Saturdays.Count() - 1; i++)
                {
                    var expectedShifts = Workplaces
                        .Select(w => new ShiftAssignment(employee, new Day(ScheduledMonth, Saturdays.ElementAt(i)), w))
                        .Concat(Workplaces.Select(w => new ShiftAssignment(employee, new Day(ScheduledMonth, Sundays.ElementAt(i)), w)))
                        .Concat(Workplaces.Select(w => new ShiftAssignment(employee, new Day(ScheduledMonth, Saturdays.ElementAt(i + 1)), w)))
                        .Concat(Workplaces.Select(w => new ShiftAssignment(employee, new Day(ScheduledMonth, Sundays.ElementAt(i + 1)), w)))
                        .ToHashSet();
                    Assert.Contains(expectedShifts, capturedShifts.Select(_ => _.ToHashSet()));
                }
            });
    }

    [Fact]
    public void NotScheduleSubsequentWeekends_FromLastMonth()
    {
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Eq, 0));

        var parameters = MonthScheduleParameters with
        {
            Month = new ScheduledMonth(2023, 1),
            CompetenceGroups = new[] {new CompetenceGroup(new[] {Workplace1})},
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, new Day(2022, 12, 31), Workplace1),
                new ShiftAssignment(Employee2, new Day(2022, 12, 25), Workplace1),
                new ShiftAssignment(Employee3, new Day(2022, 12, 24), Workplace1),
            }
        };

        var rule = new NoSubsequentWeekendsRule(parameters);
        rule.Apply(_receiverMock.Object);

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, new Day(2023, 1, 7), Workplace1),
                new ShiftAssignment(Employee1, new Day(2023, 1, 8), Workplace1)
            }.ToHashSet(), capturedShifts.Select(_ => _.ToHashSet()));

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee2, new Day(2023, 1, 1), Workplace1),
            }.ToHashSet(), capturedShifts.Select(_ => _.ToHashSet()));

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee3, new Day(2023, 1, 1), Workplace1),
            }.ToHashSet(), capturedShifts.Select(_ => _.ToHashSet()));
    }
}
