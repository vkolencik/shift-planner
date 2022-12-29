using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class EvenSpreadWithinMonthRuleShould
{
    private readonly Mock<IScheduleConstraintReceiver> _receiverMock = new Mock<IScheduleConstraintReceiver>();

    [Fact]
    public void SpreadShiftsEvenlyThroughoutMonth()
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)}), Workload.Infinite);

        var leave = new Leave(new Day(2022, 2, 1), new Day(2022, 2, 3));
        var schedulingParams = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(
                    employee,
                    new[] {leave})
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>()
        );

        List<IReadOnlyCollection<ShiftAssignment>> capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        List<IReadOnlyCollection<ShiftAssignment>> capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedLowerBounds = new List<uint>();
        var capturedUpperBounds = new List<uint>();
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, Capture.In(capturedUpperBounds)));
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, Capture.In(capturedLowerBounds)));

        // TODO: Test separately:
        var standardCalculator = new EmployeeStandardCalculator(ScheduleFlexibility.Strict, schedulingParams);
        var rule = new EvenSpreadWithinMonthRule(standardCalculator, schedulingParams);
        rule.Apply(_receiverMock.Object);

        Assert.Equal(4, capturedShiftsLte.Count);

        // TODO: 2022-12-05 min bounds relaxation
        // Assert.Equal(capturedShiftsLte.Select(_ => _.ToHashSet()), capturedShiftsGte.Select(_ => _.ToHashSet()));

        var range1 = RangeFromTo(1, 7).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range2 = RangeFromTo(8, 14).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range3 = RangeFromTo(15, 21).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        var range4 = RangeFromTo(22, 28).Select(d => new ShiftAssignment(employee, new Day(2022, 2, d), Workplace1)).ToHashSet();
        Assert.Contains(range1, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range2, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range3, capturedShiftsLte.Select(_ => _.ToHashSet()));
        Assert.Contains(range4, capturedShiftsLte.Select(_ => _.ToHashSet()));

        // TODO: 2022-12-05 min bounds relaxation
        // Assert.All(capturedLowerBounds.Skip(1), lb => Assert.Equal(7U, lb));
        Assert.All(capturedUpperBounds.Skip(1), ub => Assert.Equal(8U, ub));
        // Three-day leave in the first week => lower standard:
        // TODO: 2022-12-05 min bounds relaxation
        // Assert.Equal(4U, capturedLowerBounds.First());
        Assert.Equal(5U, capturedUpperBounds.First());
    }
    
    private static IEnumerable<int> RangeFromTo(int from, int to) => Enumerable.Range(from, to - from + 1);
}
