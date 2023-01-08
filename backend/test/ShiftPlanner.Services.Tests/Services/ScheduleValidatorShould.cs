using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Services;

public class ScheduleValidatorShould
{
    [Fact]
    public async Task ValidateSchedule_GivenValidSchedule()
    {
        var schedule = new Schedule(Array.Empty<ScheduledShift>());
        var errors = await new ScheduleValidator().ValidateSchedule(schedule);

        Assert.Empty(errors);
    } 
}
