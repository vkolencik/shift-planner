using Moq;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;
using ShiftPlanner.TestHelpers;

namespace ShiftPlanner.Services.Tests;

public class StoredScheduleServiceShould
{
    private readonly Mock<IEmployeeInfoService> _employeeInfoServiceMock;
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;

    public StoredScheduleServiceShould()
    {
        _employeeInfoServiceMock = new Mock<IEmployeeInfoService>();
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
    }

    [Fact]
    public async Task FetchSchedule()
    {
        var month = new ScheduledMonth(2023, 1);
        var day = new Day(month, 1);
        var workplace = TestData.Workplace;
        var user = new User("user-id-1", "user name", "user1@krtek.me");

        var shift = new ScheduledShift(day, workplace, user);

        _scheduleRepositoryMock
            .Setup(_ => _.LoadSchedule(month))
            .ReturnsAsync(new Schedule(new[] {shift}));

        var service = new StoredScheduleService(_employeeInfoServiceMock.Object, _scheduleRepositoryMock.Object);

        var schedule = await service.GetSchedule(month);

        Assert.Equal(new[] {shift}, schedule.Shifts);
    }
}
