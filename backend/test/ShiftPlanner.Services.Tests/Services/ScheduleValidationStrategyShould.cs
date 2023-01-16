using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Services;

public class ScheduleValidationStrategyShould
{
    private readonly Mock<IRulesetFactory> _rulesetFactoryMock = new Mock<IRulesetFactory>();
    private readonly Mock<IValidatingReceiverFactory> _validatingReceiverFactoryMock = new Mock<IValidatingReceiverFactory>();

    [Fact]
    public void ReturnSuccess_WhenAllRulesAreSatisfied()
    {
        var schedule = new Schedule(Array.Empty<ScheduledShift>());
        var parameters = TestData.MonthSchedulingParameters;
        var flexibility = new ScheduleFlexibility(333);

        var rule1Mock = new Mock<IRule>();
        var rule2Mock = new Mock<IRule>();
        _rulesetFactoryMock
            .Setup(_ => _.CreateRuleset(parameters, flexibility, It.IsAny<ISchedulingParametersRecorder>()))
            .Returns(new[] {rule1Mock.Object, rule2Mock.Object});

        var receiver1Mock = new Mock<IScheduleValidatingConstraintReceiver>();
        var receiver2Mock = new Mock<IScheduleValidatingConstraintReceiver>();
        receiver1Mock.Setup(_ => _.GetErrors()).Returns(Array.Empty<ScheduleValidationError>());
        receiver2Mock.Setup(_ => _.GetErrors()).Returns(Array.Empty<ScheduleValidationError>());
        var receiver1 = receiver1Mock.Object;
        var receiver2 = receiver2Mock.Object;
        _validatingReceiverFactoryMock.SetupSequence(_ => _.GetReceiver(schedule)).Returns(receiver1).Returns(receiver2);

        rule1Mock.Setup(_ => _.Apply(receiver1)).Verifiable();
        rule2Mock.Setup(_ => _.Apply(receiver2)).Verifiable();

        var strategy = new ScheduleValidationStrategy(schedule, parameters, _rulesetFactoryMock.Object, _validatingReceiverFactoryMock.Object);

        var isSuccess = strategy.PerformWithFlexibilityValue(flexibility, out var result);

        Assert.True(isSuccess);
        Assert.Empty(result.Errors);
        rule1Mock.Verify();
        rule2Mock.Verify();
    }

    [Fact]
    public void CollectAllErrors_WhenSomeRulesAreBroken()
    {
        var schedule = new Schedule(Array.Empty<ScheduledShift>());
        var parameters = TestData.MonthSchedulingParameters;
        var flexibility = new ScheduleFlexibility(333);

        var rule1Mock = new Mock<IRule>();
        var rule2Mock = new Mock<IRule>();
        _rulesetFactoryMock
            .Setup(_ => _.CreateRuleset(It.IsAny<IMonthScheduleParameters>(), It.IsAny<ScheduleFlexibility>(), It.IsAny<ISchedulingParametersRecorder>()))
            .Returns(new[] {rule1Mock.Object, rule2Mock.Object});

        var receiver1Mock = new Mock<IScheduleValidatingConstraintReceiver>();
        var receiver2Mock = new Mock<IScheduleValidatingConstraintReceiver>();
        var error1 = new ScheduleValidationError(Array.Empty<Shift>(), null, ScheduleErrorType.Unknown);
        var error2 = new ScheduleValidationError(Array.Empty<Shift>(), null, ScheduleErrorType.Unknown);
        receiver1Mock.Setup(_ => _.GetErrors()).Returns(new[] {error1});
        receiver2Mock.Setup(_ => _.GetErrors()).Returns(new[] {error2});
        var receiver1 = receiver1Mock.Object;
        var receiver2 = receiver2Mock.Object;
        _validatingReceiverFactoryMock
            .SetupSequence(_ => _.GetReceiver(It.IsAny<Schedule>()))
            .Returns(receiver1)
            .Returns(receiver2);

        rule1Mock.Setup(_ => _.Apply(receiver1)).Verifiable();
        rule2Mock.Setup(_ => _.Apply(receiver2)).Verifiable();

        var strategy = new ScheduleValidationStrategy(schedule, parameters, _rulesetFactoryMock.Object, _validatingReceiverFactoryMock.Object);

        var isSuccess = strategy.PerformWithFlexibilityValue(flexibility, out var result);

        Assert.False(isSuccess);
        Assert.Equal(new [] {error1, error2}, result.Errors);
    }
}
