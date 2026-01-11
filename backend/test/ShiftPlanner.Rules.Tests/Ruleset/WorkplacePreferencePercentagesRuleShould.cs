using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class WorkplacePreferencePercentagesRuleShould
{
    private readonly Mock<IScheduleConstraintReceiver> _receiverMock = new Mock<IScheduleConstraintReceiver>();

    [Fact]
    public void AssignShiftsToEmployeesAccordingToPreferencePercentages()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var employee = new Employee(
            "Employee",
            CompetenceLevel.Level1,
            new Capabilities(
                new[]
                {
                    new WorkplaceCapability.Green(
                        Workplace1,
                        20),
                    new WorkplaceCapability.Green(
                        Workplace2,
                        80)
                }),
            Workload.Infinite);
        
        var schedulingParams = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            Array.Empty<Employee>(),
            new[]
            {
                new EmployeeMonth(employee)
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1, Workplace2})
            },
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>()
        );

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count / schedulingParams.Employees.Count;

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (uint) Math.Floor(standard * 0.2)));
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (uint) Math.Ceiling(standard * 0.2)));

        // TODO: Test separately:
        var standardCalculator = new EmployeeStandardCalculator(ScheduleFlexibility.Strict, schedulingParams);
        var rule = new WorkplacePreferencePercentagesRule(standardCalculator, schedulingParams);
        
        rule.Apply(_receiverMock.Object);

        // TODO: 2022-12-05 min bounds relaxation
        // Assert.Equal(capturedShiftsGte.ToHashSet(), capturedShiftsLte.ToHashSet());
        Assert.Contains(
            schedulingParams.Month.Days.Select(d => new ShiftAssignment(employee, d, Workplace1)).ToHashSet(),
            capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x)));
    }
    
    [Fact]
    public void NotTakeRemovedWorkplacesIntoAccount()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var deactivatedWorkplace = Workplace2;
        var employee = new Employee(
            "Employee",
            CompetenceLevel.Level1,
            new Capabilities(
                new[]
                {
                    new WorkplaceCapability.Green(
                        Workplace1,
                        20),
                    new WorkplaceCapability.Green( // removed workplace (present in capabilities but not scheduled for)
                        deactivatedWorkplace,
                        80)
                }),
            Workload.Infinite);
        
        var schedulingParams = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            Array.Empty<Employee>(),
            new[]
            {
                new EmployeeMonth(employee)
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>()
        );

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, It.IsAny<uint>()));
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, It.IsAny<uint>()));

        var rule = new WorkplacePreferencePercentagesRule(Mock.Of<IEmployeeStandardCalculator>(), schedulingParams);
        
        rule.Apply(_receiverMock.Object);

        Assert.DoesNotContain(
                schedulingParams.Month.Days.Select(d => new ShiftAssignment(employee, d, deactivatedWorkplace)).ToHashSet(),
                capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x)));
        
        Assert.DoesNotContain(
                schedulingParams.Month.Days.Select(d => new ShiftAssignment(employee, d, deactivatedWorkplace)).ToHashSet(),
                capturedShiftsGte.Select(x => new HashSet<ShiftAssignment>(x)));
    }

}
