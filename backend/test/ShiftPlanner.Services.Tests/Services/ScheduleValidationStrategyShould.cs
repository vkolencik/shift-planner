using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Services;

public class ScheduleValidationStrategyShould
{
    private readonly Mock<IRulesetFactory> _rulesetFactoryMock = new Mock<IRulesetFactory>();
    
    [Fact]
    public void ReturnSuccess_WhenAllRulesAreSatisfied()
    {
        var parameters = TestData.MonthSchedulingParameters;
        var flexibility = new ScheduleFlexibility(333);
        
        var rule1Mock = new Mock<IRule>();
        var rule2Mock = new Mock<IRule>();
        _rulesetFactoryMock
            .Setup(_ => _.CreateRuleset(parameters, flexibility, It.IsAny<ISchedulingParametersRecorder>()))
            .Returns(new[] {rule1Mock.Object, rule2Mock.Object});
        
        rule1Mock.Setup(_ => _.Apply(It.IsAny<IScheduleConstraintReceiver>())).Verifiable();
        rule2Mock.Setup(_ => _.Apply(It.IsAny<IScheduleConstraintReceiver>())).Verifiable();
        
        var strategy = new ScheduleValidationStrategy(parameters, _rulesetFactoryMock.Object);

        var isSuccess = strategy.PerformWithFlexibilityValue(flexibility, out var result);
        
        Assert.True(isSuccess);
        Assert.Empty(result.Errors);
        rule1Mock.Verify();
        rule2Mock.Verify();
    }
}
