using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Services;

public class RequirementsServiceShould
{
    private readonly Mock<IRequirementsRepository> _repositoryMock;

    public RequirementsServiceShould()
    {
        _repositoryMock = new Mock<IRequirementsRepository>();
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

    private RequirementsService CreateService()
    {
        return new RequirementsService(_repositoryMock.Object);
    }
}
