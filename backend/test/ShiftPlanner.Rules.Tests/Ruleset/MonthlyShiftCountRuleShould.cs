using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules.Ruleset;

namespace ShiftPlanner.Rules.Tests.Ruleset;

using static RulesFixture;

public class MonthlyShiftCountRuleShould
{
    private readonly Mock<IScheduleConstraintReceiver> _receiverMock = new Mock<IScheduleConstraintReceiver>();
    private readonly Mock<ISchedulingParametersRecorder> _recorderMock = new Mock<ISchedulingParametersRecorder>();

    [Fact]
    public void SpreadShiftAssignmentEvenlyAmongEmployees()
    {
        var capturedShiftsGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var schedulingParams = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(Employee1),
                new EmployeeMonth(Employee2)
            },
            new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})},
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>());

        var standard = (double) schedulingParams.Month.Days.Count * schedulingParams.Workplaces.Count / schedulingParams.Employees.Count;

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsGte), Constraint.Gte, (uint) Math.Floor(standard)));
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, (uint) Math.Ceiling(standard)));

        // TODO: Test separately:
        var standardCalculator = new EmployeeStandardCalculator(ScheduleFlexibility.Strict, schedulingParams);
        var rule = new MonthlyShiftCountRule(standardCalculator, schedulingParams, _recorderMock.Object);
        rule.Apply(_receiverMock.Object);

        Assert.Equal(capturedShiftsGte.ToHashSet(), capturedShiftsLte.ToHashSet());
        Assert.All(
            schedulingParams.Employees,
            employee => Assert.Contains(
                schedulingParams.Month.Days.SelectMany(d => new[] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(employee, d, w))).ToHashSet(),
                capturedShiftsLte.Select(x => new HashSet<ShiftAssignment>(x))));
    }

    [Fact]
    public void SpreadShiftAssignmentEvenlyAmongEmployeesTakingLeavesIntoAccount()
    {
        var capturedShiftsFullMonthGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsFullMonthLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsHalfMonthOffGte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedShiftsHalfMonthOffLte = new List<IReadOnlyCollection<ShiftAssignment>>();

        var schedulingParams = new MonthScheduleParameters(
            new ScheduledMonth(2022, 2),
            new[]
            {
                new EmployeeMonth(
                    Employee1, new[]
                    {
                        new Leave(new Day(2022, 2, 1), new Day(2022, 2, 14)),
                        new Leave(new Day(2022, 2, 1), new Day(2022, 2, 14), false)
                    }),
                new EmployeeMonth(Employee2),
                new EmployeeMonth(Employee2),
            },
            new[] {new CompetenceGroup(new[] {Workplace1}), new CompetenceGroup(new[] {Workplace2})},
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>());

        var overallRequiredCapacity = (double) 2 * 28;
        var employeeCapacityWithoutLeaves = (double) 28;
        var employeeCapacityWithLeaves = (double) 14;
        var overallAvailableCapacity = (double) 2.5 * 28;
        var standardNoLeaves = overallRequiredCapacity * employeeCapacityWithoutLeaves / overallAvailableCapacity;
        var standardLeave = overallRequiredCapacity * employeeCapacityWithLeaves / overallAvailableCapacity;

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsFullMonthGte), Constraint.Gte, (uint) Math.Floor(standardNoLeaves)));
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsFullMonthLte), Constraint.Lte, (uint) Math.Ceiling(standardNoLeaves)));
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsHalfMonthOffGte), Constraint.Gte, (uint) Math.Floor(standardLeave)));
        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsHalfMonthOffLte), Constraint.Lte, (uint) Math.Ceiling(standardLeave)));

        // TODO: Test separately:
        var standardCalculator = new EmployeeStandardCalculator(ScheduleFlexibility.Strict, schedulingParams);
        var rule = new MonthlyShiftCountRule(standardCalculator, schedulingParams, _recorderMock.Object);
        rule.Apply(_receiverMock.Object);

        Assert.Equal(capturedShiftsFullMonthGte.ToHashSet(), capturedShiftsFullMonthLte.ToHashSet());
        Assert.Equal(capturedShiftsHalfMonthOffGte.ToHashSet(), capturedShiftsHalfMonthOffLte.ToHashSet());

        Assert.Contains(
            schedulingParams.Month.Days
                .SelectMany(d => new[] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(Employee1, d, w)))
                .ToHashSet(),
            capturedShiftsHalfMonthOffLte.Select(_ => _.ToHashSet()));

        Assert.Contains(
            schedulingParams.Month.Days
                .SelectMany(d => new[] {Workplace1, Workplace2}.Select(w => new ShiftAssignment(Employee2, d, w)))
                .ToHashSet(),
            capturedShiftsFullMonthLte.Select(_ => _.ToHashSet()));
    }
    
    [Theory]
    [MemberData(nameof(GetWorkloads))]
    public void TakeEmployeeMaxStandardIntoAccount(Workload workload)
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)}), workload);
        var capturedShiftsLte = new List<IReadOnlyCollection<ShiftAssignment>>();
        var capturedBoundsLte = new List<uint>();

        var month = new ScheduledMonth(2022, 2);
        var schedulingParams = new MonthScheduleParameters(
            month,
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

        _receiverMock.Setup(_ => _.AddSumConstraint(Capture.In(capturedShiftsLte), Constraint.Lte, Capture.In(capturedBoundsLte)));

        // TODO: Test separately:
        var standardCalculator = new EmployeeStandardCalculator(ScheduleFlexibility.Strict, schedulingParams);
        var rule = new MonthlyShiftCountRule(standardCalculator, schedulingParams, _recorderMock.Object);
        rule.Apply(_receiverMock.Object);

        var lteBound = Assert.Single(capturedBoundsLte);
        var lteShifts = Assert.Single(capturedShiftsLte);

        Assert.Equal(workload.MaximumShiftsPerMonth, lteBound);
        Assert.Equal(month.Days.Select(d => new ShiftAssignment(employee, d, Workplace1)), lteShifts);
    }

    [Theory]
    [MemberData(nameof(GetWorkloads))]
    public void TakeEmployeeMaxStandardIntoAccountIncludingLeave(Workload workload)
    {
        var employee = new Employee("Employee", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)}), workload);
        var capturedBoundsLte = new List<uint>();
        var capturedBoundsGte = new List<uint>();

        var month = new ScheduledMonth(2022, 2);
        var schedulingParams = new MonthScheduleParameters(
            month,
            new[]
            {
                new EmployeeMonth(employee, new[] {new Leave(new Day(2022, 2, 1), new Day(2022, 2, 14))})
            },
            new[]
            {
                new CompetenceGroup(new[] {Workplace1})
            },
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>()
        );

        _receiverMock.Setup(_ => _.AddSumConstraint(It.IsAny<IReadOnlyCollection<ShiftAssignment>>(), Constraint.Lte, Capture.In(capturedBoundsLte)));
        _receiverMock.Setup(_ => _.AddSumConstraint(It.IsAny<IReadOnlyCollection<ShiftAssignment>>(), Constraint.Gte, Capture.In(capturedBoundsGte)));

        // TODO: Test separately:
        var standardCalculator = new EmployeeStandardCalculator(ScheduleFlexibility.Strict, schedulingParams);
        var rule = new MonthlyShiftCountRule(standardCalculator, schedulingParams, _recorderMock.Object);
        rule.Apply(_receiverMock.Object);

        var lteBound = Assert.Single(capturedBoundsLte);
        var gteBound = Assert.Single(capturedBoundsGte);

        Assert.Equal(Math.Ceiling((double) workload.MaximumShiftsPerMonth / 2), lteBound);

        // TODO: create a separate test for upper bound as well
        // Assert.Equal(Math.Floor((double) workload.MaximumShiftsPerMonth / 2), gteBound);
    }

    public static IEnumerable<object[]> GetWorkloads() => new[]
    {
        new Workload(0, 1),
        new Workload(4, 5),
        new Workload(7, 8),
        new Workload(8, 8)
    }.Select(_ => new object[] {_});
}
