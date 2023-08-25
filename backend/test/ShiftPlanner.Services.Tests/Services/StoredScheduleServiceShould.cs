using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Persistence;
using ShiftPlanner.Services.Services;

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
        var user = TestData.TestUser();

        var shift = new ScheduledShift(day, workplace, user);

        _scheduleRepositoryMock
            .Setup(_ => _.LoadSchedule(month))
            .ReturnsAsync(new Schedule(new[] {shift}));

        var service = new StoredScheduleService(_scheduleRepositoryMock.Object);

        var schedule = await service.GetSchedule(month);

        Assert.Equal(new[] {shift}, schedule.Shifts);
    }

    [Fact]
    public async Task StoreSchedule()
    {
        var tenant = TestData.Tenant;
        var month = new ScheduledMonth(2023, 1);
        var day = new Day(month, 1);
        var workplace = TestData.Workplace;
        var user = TestData.TestUser();
        var schedule = new Schedule(new [] { new ScheduledShift(day, workplace, user) });

        _scheduleRepositoryMock
            .Setup(_ => _.StoreSchedule(month, schedule));

        var service = new StoredScheduleService(_scheduleRepositoryMock.Object);

        await service.StoreSchedule(tenant, month, schedule);
    }
}
