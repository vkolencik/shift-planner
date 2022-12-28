using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class AlwaysScheduleAlternatesOnWeekendsRuleShould
{
    private readonly Mock<IScheduleConstraintReceiver> _receiverMock = new Mock<IScheduleConstraintReceiver>();

    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends()
    {
        var capturedShiftsLhs = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsRhs = new List<IReadOnlyCollection<ShiftAssignment>>();

        _receiverMock.Setup(_ => _.AddSumRelationConstraint(Capture.In(capturedShiftsLhs), Constraint.Eq, Capture.In(capturedShiftsRhs)));

        var rule = new AlwaysScheduleAlternatesOverWeekendsRule(MonthScheduleParameters);
        rule.Apply(_receiverMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                Assert.All(
                    Workplaces, workplace =>
                    {
                        Assert.All<(int, int)>(
                            Fridays.Zip(Sundays), tuple =>
                            {
                                var (friday, sunday) = tuple;
                                Assert.Contains(
                                    new HashSet<ShiftAssignment> {new ShiftAssignment(employee, new Day(ScheduledMonth, friday), workplace)},
                                    capturedShiftsLhs.Select(_ => _.ToHashSet()));
                                Assert.Contains(
                                    new HashSet<ShiftAssignment> {new ShiftAssignment(employee, new Day(ScheduledMonth, sunday), workplace)},
                                    capturedShiftsRhs.Select(_ => _.ToHashSet()));
                            });
                    });
            });
    }

    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends_WithInterchangeability()
    {
        var capturedShiftsLhs = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsRhs = new List<IReadOnlyCollection<ShiftAssignment>>();

        _receiverMock.Setup(_ => _.AddSumRelationConstraint(Capture.In(capturedShiftsLhs), Constraint.Eq, Capture.In(capturedShiftsRhs)));

        var fridays = ScheduledMonth.Days.Where(d => d.DayOfWeek == DayOfWeek.Friday).Select(d => d.DayOfMonth);
        var sundays = ScheduledMonth.Days.SkipWhile(d => d.DayOfWeek != DayOfWeek.Friday).Where(d => d.DayOfWeek == DayOfWeek.Sunday).Select(d => d.DayOfMonth);

        var monthScheduleParameters = MonthScheduleParameters with
        {
            WeekendInterchangeabilityGroups = new[]
            {
                new HashSet<Workplace> {Workplace1, Workplace2}
            }
        };

        var rule = new AlwaysScheduleAlternatesOverWeekendsRule(monthScheduleParameters);
        rule.Apply(_receiverMock.Object);

        Assert.All(
            new[] {Employee1, Employee2, Employee3}, employee =>
            {
                Assert.All(
                    new[] {Workplace1, Workplace2}, workplace =>
                    {
                        Assert.All<(int, int)>(
                            fridays.Zip(sundays), tuple =>
                            {
                                var (friday, sunday) = tuple;
                                Assert.Contains(
                                    new HashSet<ShiftAssignment>
                                    {
                                        new ShiftAssignment(employee, new Day(ScheduledMonth, friday), Workplace1),
                                        new ShiftAssignment(employee, new Day(ScheduledMonth, friday), Workplace2),
                                    },
                                    capturedShiftsLhs.Select(_ => _.ToHashSet()));
                                Assert.Contains(
                                    new HashSet<ShiftAssignment>
                                    {
                                        new ShiftAssignment(employee, new Day(ScheduledMonth, sunday), Workplace1),
                                        new ShiftAssignment(employee, new Day(ScheduledMonth, sunday), Workplace2),
                                    },
                                    capturedShiftsRhs.Select(_ => _.ToHashSet()));
                            });
                    });
            });
    }
    
    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends_FromLastMonth()
    {
        var capturedShiftsVariable = new List<IReadOnlyCollection<ShiftAssignment>>();

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsVariable), Constraint.Eq, 1));

        var month = new ScheduledMonth(2022, 1);

        var monthScheduleParameters = MonthScheduleParameters with
        {
            Month = month,
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, new Day(2021, 12, 31), Workplace1), // Friday 
            }
        };

        var rule = new AlwaysScheduleAlternatesOverWeekendsRule(monthScheduleParameters);
        rule.Apply(_receiverMock.Object);

        Assert.Equal(DayOfWeek.Friday, month.Previous.Days.Last().DayOfWeek);
        Assert.Contains(new List<ShiftAssignment> {new ShiftAssignment(Employee1, new Day(month, 2), Workplace1)}, capturedShiftsVariable);
    }

    [Fact]
    public void AlwaysScheduleAlternatesOverWeekends_FromLastMonth_WithInterchangeability()
    {
        var capturedShiftsVariable = new List<IReadOnlyCollection<ShiftAssignment>>();

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsVariable), Constraint.Eq, 1));

        var month = new ScheduledMonth(2022, 1);

        var monthScheduleParameters = MonthScheduleParameters with
        {
            Month = month,
            LastMonthShifts = new[]
            {
                new ShiftAssignment(Employee1, new Day(2021, 12, 31), Workplace1), // Friday 
            },
            WeekendInterchangeabilityGroups = new[]
            {
                new HashSet<Workplace> {Workplace1, Workplace2}
            }
        };

        var rule = new AlwaysScheduleAlternatesOverWeekendsRule(monthScheduleParameters);
        rule.Apply(_receiverMock.Object);

        Assert.Equal(DayOfWeek.Friday, month.Previous.Days.Last().DayOfWeek);
        Assert.Contains(
            new List<ShiftAssignment>
            {
                new ShiftAssignment(Employee1, new Day(month, 2), Workplace1),
                new ShiftAssignment(Employee1, new Day(month, 2), Workplace2),
            }, capturedShiftsVariable);
    }

}
