using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Services;

public class ScheduleValidatorShould
{
    [Fact]
    public async Task ValidateSchedule()
    {
        await new ScheduleValidator().ValidateSchedule(null!);
    } 
}
