using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Services;

public class ScheduleValidatorShould
{
    private readonly Mock<ISchedulingParametersAggregator> _paramsAggregatorMock = new Mock<ISchedulingParametersAggregator>();
    private readonly Mock<IFlexibilityVariator> _flexibilityVariatorMock = new Mock<IFlexibilityVariator>();
    private readonly Mock<IRulesetFactory> _rulesetFactoryMock = new Mock<IRulesetFactory>();

    [Fact]
    public async Task ValidateSchedule_GivenValidSchedule()
    {
        var tenant = TestData.Tenant;
        var schedule = new Schedule(Array.Empty<ScheduledShift>());
        var month = new ScheduledMonth(2000, 1);
        var parameters = TestData.MonthSchedulingParameters;
        
        _paramsAggregatorMock
            .Setup(_ => _.GetSchedulingParameters(tenant, month))
            .ReturnsAsync(parameters);

        var capturedStrategies = new List<IVariableFlexibilityStrategy<ScheduleValidationResult>>();
        _flexibilityVariatorMock
            .Setup(_ => _.ExecuteWithVariableFlexibility(Capture.In(capturedStrategies)))
            .Returns(new VariableFlexibilityCalculationResult<ScheduleValidationResult>(new ScheduleValidationResult(Array.Empty<ScheduleValidationError>()), true, ScheduleFlexibility.Strict));

        var validator = CreateValidator();
        
        var errors = await validator.ValidateSchedule(tenant, month, schedule);

        Assert.Empty(errors);
        var strategy = Assert.Single(capturedStrategies);
        var validationStrategy = Assert.IsType<ScheduleValidationStrategy>(strategy);
        Assert.Equal(parameters, validationStrategy.Parameters);
    }

    [Fact]
    public async Task ReturnErrors_GivenInvalidSchedule()
    {
        var tenant = TestData.Tenant;
        var schedule = new Schedule(Array.Empty<ScheduledShift>());
        var month = new ScheduledMonth(2000, 1);
        
        var expectedErrors = new [] { new ScheduleValidationError(Array.Empty<Shift>(), null, ScheduleErrorType.Unknown) };

        _flexibilityVariatorMock
            .Setup(_ => _.ExecuteWithVariableFlexibility(It.IsAny<IVariableFlexibilityStrategy<ScheduleValidationResult>>()))
            .Returns(new VariableFlexibilityCalculationResult<ScheduleValidationResult>(new ScheduleValidationResult(expectedErrors), false, ScheduleFlexibility.Strict));

        var validator = CreateValidator();
        
        var errors = await validator.ValidateSchedule(tenant, month, schedule);

        Assert.Equal(expectedErrors, errors);
    }

    private ScheduleValidator CreateValidator()
    {
        return new ScheduleValidator(_paramsAggregatorMock.Object, _flexibilityVariatorMock.Object, _rulesetFactoryMock.Object);
    }
}
