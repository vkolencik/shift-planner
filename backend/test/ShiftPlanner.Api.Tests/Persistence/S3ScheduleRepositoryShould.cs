using Amazon.S3;
using Microsoft.Extensions.Options;
using Moq;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Persistence;

namespace ShiftPlanner.Api.Tests.Persistence;

public class S3ScheduleRepositoryShould
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<IUserService> _userServiceMock;

    public S3ScheduleRepositoryShould()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _userServiceMock = new Mock<IUserService>();
    }

    [Fact(Skip = "Will be implemented")]
    public async Task StoreSchedule()
    {
        var repository = CreateRepository();

        await repository.StoreComputedSchedule(
            new ScheduledMonth(2023, 1),
            new SuccessfulSchedulingResult(
                PlannedSchedule.CreateSchedule(new ShiftAssignment[] { }),
                ScheduleFlexibility.Strict,
                new SolutionParameters(new Dictionary<Employee, EmployeeStandard>())));
    }

    [Fact(Skip = "Will be implemented")]
    public async Task LoadLatestSchedule()
    {
        var repository = CreateRepository();

        var schedule = await repository.LoadSchedule(new ScheduledMonth(2023, 1));
    }

    private S3ScheduleRepository CreateRepository()
    {
        return new S3ScheduleRepository(
            _s3ClientMock.Object,
            _userServiceMock.Object,
            Options.Create(new StorageOptions {StoragePrefix = "_test123"}));
    }
}
