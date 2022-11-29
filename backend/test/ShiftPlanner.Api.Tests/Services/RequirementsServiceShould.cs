using Moq;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Services;

public class RequirementsServiceShould
{
    private readonly Mock<IRequirementsRepository> _repositoryMock;
    private readonly Mock<IUserService> _userServiceMock;

    public RequirementsServiceShould()
    {
        _repositoryMock = new Mock<IRequirementsRepository>();
        _userServiceMock = new Mock<IUserService>();
    }

    [Fact]
    public async Task GetRequirements()
    {
        var tenant = TestData.Tenant;
        var userId = "user-id";
        var month = new ScheduledMonth(2022, 10);
        var requirements = TestData.Requirements;

        _repositoryMock.Setup(_ => _.GetRequirements(tenant, userId, month)).ReturnsAsync(requirements);

        var service = CreateService();

        var returnedRequirements = await service.GetRequirements(tenant, userId, month);

        Assert.Equal(requirements, returnedRequirements);
    }

    [Fact]
    public async Task SetRequirement()
    {
        var tenant = TestData.Tenant;
        var userId = "user-id";
        var day = new Day(2022, 10, 18);
        var requirement = "hard-no";

        var service = CreateService();

        await service.SetRequirement(tenant, userId, day, requirement);

        _repositoryMock.Verify(_ => _.SetRequirement(tenant, userId, day, requirement));
    }

    [Theory]
    [InlineData(DeleteResult.Deleted)]
    [InlineData(DeleteResult.NotFound)]
    public async Task DeleteRequirement(DeleteResult repositoryResult)
    {
        var tenant = TestData.Tenant;
        var userId = "user-id";
        var day = new Day(2022, 10, 18);
        _repositoryMock.Setup(_ => _.DeleteRequirement(tenant, userId, day))
            .ReturnsAsync(repositoryResult)
            .Verifiable();

        var service = CreateService();

        var serviceResult = await service.DeleteRequirement(tenant, userId, day);

        _repositoryMock.VerifyAll();
        Assert.Equal(repositoryResult, serviceResult);
    }

    [Fact]
    public async Task GetRequirementsOverviewForMonth()
    {
        var tenant = TestData.Tenant;
        var month = TestData.Month;
        
        var expectedRequirements = new[]
        {
            new UserRequirementModel("user1", "User 1", new Day(2022, 12, 2), "hard-no"),
            new UserRequirementModel("user2", "User 2", new Day(2022, 12, 3), "soft-yes"),
        };

        _repositoryMock
            .Setup(_ => _.GetRequirementsForMonth(tenant, month))
            .ReturnsAsync(
                new[]
                {
                    new RequirementModel("user1", new Day(2022, 12, 2), "hard-no"),
                    new RequirementModel("user2", new Day(2022, 12, 3), "soft-yes"),
                });

        _userServiceMock
            .Setup(_ => _.GetAllUsers())
            .ReturnsAsync(new []
            {
                new User("user1", "User 1", "user1@krtek.me"), 
                new User("user2", "User 2", "user2@krtek.me"), 
            });
        var service = CreateService();

        var requirements = await service.GetRequirementsOverviewForMonth(tenant, month);

        Assert.Equal(expectedRequirements, requirements);
    }

    [Fact]
    public async Task ApproveRequirement()
    {
        Tenant tenant = TestData.Tenant;
        string userId = "user-123";
        Day day = TestData.Day;
        RequirementModel requirement = new RequirementModel("user-123", day, "leave");
        
        _repositoryMock
            .Setup(_ => _.GetRequirement(tenant, userId, day))
            .ReturnsAsync(requirement);
        var service = CreateService();

        var result = service.Approve(TestData.Tenant, "userId", TestData.Day);
        
        
        
    }

    private RequirementsService CreateService()
    {
        return new RequirementsService(_repositoryMock.Object, _userServiceMock.Object);
    }
}
