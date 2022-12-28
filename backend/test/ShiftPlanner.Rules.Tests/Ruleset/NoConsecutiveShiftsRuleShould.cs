using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static ShiftPlanner.Rules.Tests.RulesFixture;

public class NoConsecutiveShiftsRuleShould
{
    [Fact]
    public void DisallowConsecutiveShiftsForEmployee()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();

        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsZero = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsZero), Constraint.Eq, 0));

        var previousMonthDays = ScheduledMonth.Previous.Days;
        var parameters = MonthScheduleParameters with
        {
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, previousMonthDays.ElementAt(previousMonthDays.Count - 1), Workplace1),
            }
        };

        var rule = new NoConsecutiveShiftsRule(parameters);
        rule.Apply(receiverMock.Object);

        Assert.Equal(parameters.Employees.Count * (ScheduledMonth.Days.Count - 1), capturedShifts.Count);
        var day1 = ScheduledMonth.Days.ElementAt(0);
        var day2 = ScheduledMonth.Days.ElementAt(1);

        // consecutive shift within month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, day1, Workplace1),
                new ShiftAssignment(Employee1, day1, Workplace2),
                new ShiftAssignment(Employee1, day2, Workplace1),
                new ShiftAssignment(Employee1, day2, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        // consecutive shift within month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee2, day1, Workplace1),
                new ShiftAssignment(Employee2, day1, Workplace2),
                new ShiftAssignment(Employee2, day2, Workplace1),
                new ShiftAssignment(Employee2, day2, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        // consecutive shift from last month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, Day1, Workplace1),
                new ShiftAssignment(Employee1, Day1, Workplace2),
            }, capturedShiftsZero.Select(x => new HashSet<ShiftAssignment>(x)));
    }
}
