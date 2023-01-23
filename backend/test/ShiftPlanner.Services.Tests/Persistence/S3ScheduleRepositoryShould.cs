using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Persistence;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Persistence;

public class S3ScheduleRepositoryShould
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IEmployeeInfoService> _employeeInfoServiceMock;

    public S3ScheduleRepositoryShould()
    {
        _employeeInfoServiceMock = new Mock<IEmployeeInfoService>();
        _s3ClientMock = new Mock<IAmazonS3>(MockBehavior.Strict);
        _userServiceMock = new Mock<IUserService>();
        _employeeInfoServiceMock = new Mock<IEmployeeInfoService>();
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

    [Fact]
    public async Task ReturnNull_WhenScheduleDoesNotExist()
    {
        var repository = CreateRepository();

        _s3ClientMock
            .Setup(_ => _.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ListObjectsV2Response
                {
                    S3Objects = new List<S3Object>
                    {
                        new S3Object {Key = "2023/06/", Size = 0, LastModified = DateTime.Now.AddHours(-1)}, // Folder object
                    }
                });
        
        var schedule = await repository.LoadSchedule(new ScheduledMonth(2023, 6));

        Assert.Null(schedule);
    }
    
    [Fact]
    public async Task LoadLatestSchedule()
    {
        var prefix = "2023/06/";
        var storedScheduleKey = prefix + "abc123.json";
        var userId = "user-id-1";
        var user = new User(userId, "John Doe", "jdoe@krtek.me");

        var storedSchedule = new {Shifts = new[] {new {Day = "2023-06-01", Workplace = "X7", UserId = userId}}};

        _s3ClientMock
            .Setup(_ => _.ListObjectsV2Async(It.Is<ListObjectsV2Request>(r =>
                    r.BucketName == "test123-schedule-storage-720500131615" 
                    && r.Prefix == prefix)
                , It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new ListObjectsV2Response
                {
                    S3Objects = new List<S3Object>
                    {
                        new S3Object {Key = prefix, Size = 0, LastModified = DateTime.Now.AddHours(-1)}, // Folder object
                        new S3Object {Key = storedScheduleKey, Size = 12345, LastModified = DateTime.Now.AddHours(-2)}
                    }
                })
            .Verifiable();

        _s3ClientMock
            .Setup(_ => _.GetObjectAsync(It.Is<GetObjectRequest>(r => r.BucketName == "test123-schedule-storage-720500131615" && r.Key == storedScheduleKey), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectResponse() {ResponseStream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(storedSchedule))});

        _userServiceMock.Setup(_ => _.GetAllUsers()).ReturnsAsync(new[] {user});
        
        var repository = CreateRepository();

        var schedule = await repository.LoadSchedule(new ScheduledMonth(2023, 6));

        _s3ClientMock.VerifyAll();
        Assert.Equal(new[] {new ScheduledShift(new Day(2023, 6, 1), new Workplace("X7"), user),}, schedule.Shifts);
    }

    private S3ScheduleRepository CreateRepository()
    {
        return new S3ScheduleRepository(
            _s3ClientMock.Object,
            _userServiceMock.Object,
            Options.Create(new StorageOptions {StoragePrefix = "_test123-"}),
            _employeeInfoServiceMock.Object);
    }
}
