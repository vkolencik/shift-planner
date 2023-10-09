using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

public class CoverEveryDayRuleShould
{
    [Fact]
    public void CoverEveryDayAtEveryWorkplace()
    {
        var receiverMock = new Mock<IScheduleConstraintReceiver>();

        var capturedShifts = new List<IReadOnlyCollection<ShiftAssignment>>();
        receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShifts), Constraint.Eq, 1));

        Day Day1 = new Day(2022, 2, 1);
        Day Day2 = new Day(2022, 2, 2);
        Workplace Workplace1 = new Workplace("Workplace1");
        Workplace Workplace2 = new Workplace("Workplace2");
        Workplace Workplace3 = new Workplace("Workplace3");
        Employee Employee1 = new Employee("Employee1", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)}), Workload.Infinite);
        Employee Employee2 = new Employee("Employee2", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace2, 100)}), Workload.Infinite);

        var solverParameters = new SolverParameters(
            new[] {Day1, Day2},
            new[] {Employee1, Employee2},
            new[] {Workplace1, Workplace2, Workplace3});

        var rule = new EveryDayAtEveryWorkplaceIsCoveredRule(
            solverParameters,
            new[] {Workplace3});

        rule.Apply(receiverMock.Object);

        Assert.Equal(4, capturedShifts.Count);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day1, RulesFixture.Workplace1), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day1, RulesFixture.Workplace1)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day1, RulesFixture.Workplace2), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day1, RulesFixture.Workplace2)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day2, RulesFixture.Workplace1), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day2, RulesFixture.Workplace1)}, capturedShifts);
        Assert.Contains(new[] {new ShiftAssignment(RulesFixture.Employee1, RulesFixture.Day2, RulesFixture.Workplace2), new ShiftAssignment(RulesFixture.Employee2, RulesFixture.Day2, RulesFixture.Workplace2)}, capturedShifts);
    }
}
