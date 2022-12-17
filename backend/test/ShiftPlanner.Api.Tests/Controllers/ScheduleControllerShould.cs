using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.ScheduleCalculator;
using ShiftPlanner.TestHelpers;

namespace ShiftPlanner.Api.Tests.Controllers;

public class ScheduleControllerShould
{
    private readonly Mock<IStoredScheduleService> _storedScheduleServiceMock = new Mock<IStoredScheduleService>();
    
    [Fact]
    public async Task GetSchedule()
    {
        var controller = new ScheduleController();
        var workplace = TestData.Workplace;
        var month = new ScheduledMonth(2022, 1);

        var scheduledShift = new ScheduledShift(new Day(month, 1), workplace, new User("user1", "user 1", "user1@krtek.me"));
        var storedSchedule = new StoredSchedule(new [] {scheduledShift});

        _storedScheduleServiceMock
            .Setup(_ => _.GetSchedule(month))
            .ReturnsAsync(storedSchedule);
        
        var response = await controller.GetSchedule(2027, 7);

        Assert.IsType<OkObjectResult>(response.Result);

        Assert.Equal(response.Value, storedSchedule);

    }
}
