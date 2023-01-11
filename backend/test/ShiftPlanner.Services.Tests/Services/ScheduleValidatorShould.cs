using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Services;

public class ScheduleValidatorShould
{
    private readonly Mock<ISchedulingParametersAggregator> _paramsAggregatorMock = new Mock<ISchedulingParametersAggregator>();
    private readonly Mock<IVariableFlexibilityStrategy<ScheduleValidationResult>> _variatorStrategyMock = new Mock<IVariableFlexibilityStrategy<ScheduleValidationResult>>();
    
    [Fact]
    public async Task ValidateSchedule_GivenValidSchedule()
    {
        var tenant = TestData.Tenant;
        var schedule = new Schedule(Array.Empty<ScheduledShift>());
        var month = new ScheduledMonth(2000, 1);
        var parameters = new MonthScheduleParameters(
            month, Array.Empty<EmployeeMonth>(), Array.Empty<CompetenceGroup>(), Array.Empty<ShiftAssignment>(), Array.Empty<ShiftAssignment>(), Array.Empty<ISet<Workplace>>()
        );
        
        _paramsAggregatorMock
            .Setup(_ => _.GetSchedulingParameters(tenant, month))
            .ReturnsAsync(parameters);

        var validator = CreateValidator();
        
        var errors = await validator.ValidateSchedule(tenant, month, schedule);

        Assert.Empty(errors);
    }

    private ScheduleValidator CreateValidator()
    {
        return new ScheduleValidator(_paramsAggregatorMock.Object);
    }
}
