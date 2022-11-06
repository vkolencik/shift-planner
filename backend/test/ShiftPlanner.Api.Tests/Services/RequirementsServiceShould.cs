using Moq;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Services;

public class RequirementsServiceShould
{
    private Mock<IRequirementsRepository> _repositoryMock;

    public RequirementsServiceShould()
    {
        _repositoryMock = new Mock<IRequirementsRepository>();
    }

    [Fact]
    public async Task GetRequirements()
    {
        var tenant = new Tenant("tenant1");
        var userId = "user-id";
        var month = new ScheduledMonth(2022, 10);
        var requirements = TestData.Requirements;

        _repositoryMock.Setup(_ => _.GetRequirements(tenant, userId, month)).ReturnsAsync(requirements);
        var service = new RequirementsService(_repositoryMock.Object);
        
        var returnedRequirements = await service.GetRequirements(tenant, userId, month);

        Assert.Equal(requirements, returnedRequirements);
    }
}
