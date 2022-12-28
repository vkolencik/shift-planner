using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;
using static RulesFixture;

public class NoAlternateDayShiftsRuleShould
{
    [Fact]
    public void DisallowShiftsOnAlternateDaysExceptWeekends()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();
        
        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Lte, 1));

        var day1 = ScheduledMonth.Days.ElementAt(0);
        var day3 = ScheduledMonth.Days.ElementAt(2);
        var firstFriday = ScheduledMonth.Days.First(d => d.DayOfWeek == DayOfWeek.Friday);
        var firstSundayAfterFriday = ScheduledMonth.Days.SkipWhile(d => d.DayOfWeek != DayOfWeek.Friday).First(d => d.DayOfWeek == DayOfWeek.Sunday);

        var monthScheduleParameters = MonthScheduleParameters;

        var rule = new NoAlternativeDayShiftsRule(monthScheduleParameters);
        rule.Apply(receiverMock.Object);

        // alternating shift within month:
        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, day1, Workplace1),
                new ShiftAssignment(Employee1, day1, Workplace2),
                new ShiftAssignment(Employee1, day3, Workplace1),
                new ShiftAssignment(Employee1, day3, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));

        // alternating shift within month:
        Assert.DoesNotContain(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, firstFriday, Workplace1),
                new ShiftAssignment(Employee1, firstFriday, Workplace2),
                new ShiftAssignment(Employee1, firstSundayAfterFriday, Workplace1),
                new ShiftAssignment(Employee1, firstSundayAfterFriday, Workplace2)
            }, capturedShifts.Select(x => new HashSet<ShiftAssignment>(x)));
    }
    
    [Fact]
    public void DisallowShiftsOnAlternateDaysExceptWeekends_FromLastMonth()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();
        
        var capturedShiftsZero = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsZero), Constraint.Eq, 0));

        var scheduledMonth = new ScheduledMonth(2022, 1);
        var day1 = scheduledMonth.Days.ElementAt(0);
        var day2 = scheduledMonth.Days.ElementAt(1);
        var previousMonthDays = scheduledMonth.Previous.Days;

        var monthScheduleParameters = MonthScheduleParameters with
        {
            Month = scheduledMonth,
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, previousMonthDays.ElementAt(previousMonthDays.Count - 1), Workplace1), // Friday
                new ShiftAssignment(Employee2, previousMonthDays.ElementAt(previousMonthDays.Count - 2), Workplace1), // Thursday
            }
        };

        var rule = new NoAlternativeDayShiftsRule(monthScheduleParameters);
        rule.Apply(receiverMock.Object);

        // alternating shifts from last month:
        Assert.DoesNotContain(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, day2, Workplace1), // Sunday
                new ShiftAssignment(Employee1, day2, Workplace2),
            }, capturedShiftsZero.Select(x => new HashSet<ShiftAssignment>(x)));

        Assert.Contains(
            new HashSet<ShiftAssignment>
            {
                new ShiftAssignment(Employee2, day1, Workplace1), // Saturday
                new ShiftAssignment(Employee2, day1, Workplace2),
            }, capturedShiftsZero.Select(x => new HashSet<ShiftAssignment>(x)));
    }


}
