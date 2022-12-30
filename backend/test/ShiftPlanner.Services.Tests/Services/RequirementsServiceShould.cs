using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Persistence;
using ShiftPlanner.Services.Services;
using TestData = ShiftPlanner.Services.Tests.TestData;

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
        var requirements = TestData.MonthRequirements;

        _repositoryMock.Setup(_ => _.GetRequirements(tenant, userId, month)).ReturnsAsync(requirements);

        var service = CreateService();

        var returnedRequirements = await service.GetRequirements(tenant, userId, month);

        Assert.Equal(requirements, returnedRequirements);
    }

    [Fact]
    public async Task SetRequirement()
    {
        var requirement = TestData.Requirement;
        var service = CreateService();

        await service.SetRequirement(requirement);

        _repositoryMock.Verify(_ => _.SetRequirement(requirement));
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
            new UserRequirementModel("user1", "User 1", new Day(2022, 12, 2), "hard-no", null),
            new UserRequirementModel("user2", "User 2", new Day(2022, 12, 3), "soft-yes", null),
            new UserRequirementModel("user2", "User 2", new Day(2022, 12, 4), "leave", true),
        };

        _repositoryMock
            .Setup(_ => _.GetRequirementsForMonth(tenant, month))
            .ReturnsAsync(
                new[]
                {
                    new Requirement(TestData.Tenant, "user1", new Day(2022, 12, 2), "hard-no"),
                    new Requirement(TestData.Tenant, "user2", new Day(2022, 12, 3), "soft-yes"),
                    new Requirement(TestData.Tenant, "user2", new Day(2022, 12, 4), "leave", true),
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
        var tenant = TestData.Tenant;
        var userId = "user-123";
        var day = TestData.Day;
        var requirementFromDb = new Requirement(TestData.Tenant, userId, day, "leave");
        var updatedRequirement = new Requirement(TestData.Tenant, userId, day, "leave", true);
        
        _repositoryMock
            .Setup(_ => _.GetRequirement(tenant, userId, day))
            .ReturnsAsync(requirementFromDb);
        _repositoryMock
            .Setup(_ => _.SetRequirement(updatedRequirement))
            .Verifiable();

        var service = CreateService();

        var result = await service.Approve(TestData.Tenant, userId, TestData.Day);
        
        _repositoryMock.VerifyAll();
        Assert.Equal(ApproveResult.Success, result);
    }

    [Fact]
    async Task ReturnError_WhenApprovingRequirementThatIsNotLeave()
    {
        
        _repositoryMock
            .Setup(_ => _.GetRequirement(It.IsAny<Tenant>(), It.IsAny<string>(), It.IsAny<Day>()))
            .ReturnsAsync(TestData.Requirement with {RequirementType = "hard-no"});
        var service = CreateService();
        
        var result = await service.Approve(TestData.Tenant, "user-1", TestData.Day);

        Assert.Equal(ApproveResult.Failure, result);
    }

    [Fact]
    async Task ReturnNotFound_WhenRequirementDoesNotExist()
    {
        _repositoryMock
            .Setup(_ => _.GetRequirement(It.IsAny<Tenant>(), It.IsAny<string>(), It.IsAny<Day>()))
            .ReturnsAsync((Requirement?) null);
        var service = CreateService();
        
        var result = await service.Approve(TestData.Tenant, "user-1", TestData.Day);

        Assert.Equal(ApproveResult.NotFound, result);
    }

    private RequirementsService CreateService()
    {
        return new RequirementsService(_repositoryMock.Object, _userServiceMock.Object);
    }
}
