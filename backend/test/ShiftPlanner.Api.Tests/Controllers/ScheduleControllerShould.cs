using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;
using ShiftPlanner.ScheduleCalculator;
using ShiftPlanner.Services;
using ShiftPlanner.TestHelpers;

namespace ShiftPlanner.Api.Tests.Controllers;

public class ScheduleControllerShould
{
    private readonly Mock<IStoredScheduleService> _storedScheduleServiceMock = new Mock<IStoredScheduleService>();

    [Fact]
    public async Task GetSchedule()
    {
        var workplace = new Workplace("ABC");
        var month = new ScheduledMonth(2027, 7);

        var scheduledShift = new ScheduledShift(new Day(month, 3), workplace, new User("user1", "Mr. User 1", "user1@krtek.me"));
        var storedSchedule = new Schedule(new[] {scheduledShift});
        var expectedOutputShifts = new[] {new ScheduleController.ShiftOutputModel("2027-07-03", "ABC", new UserOutputModel("user1", "Mr. User 1"))};

        _storedScheduleServiceMock
            .Setup(_ => _.GetSchedule(month))
            .ReturnsAsync(storedSchedule);

        var controller = new ScheduleController(_storedScheduleServiceMock.Object);

        var response = await controller.GetSchedule(2027, 7);

        var result = Assert.IsType<OkObjectResult>(response.Result);

        Assert.Equal(expectedOutputShifts, ((ScheduleController.ScheduleOutputModel?) result.Value)?.Shifts);
    }
}
