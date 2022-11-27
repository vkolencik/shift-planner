using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Controllers;

public class RequirementsControllerShould
{
    private Mock<IRequirementsService> _requirementsServiceMock;

    public RequirementsControllerShould()
    {
        _requirementsServiceMock = new Mock<IRequirementsService>();
    }

    [Fact]
    public async Task ReturnRequirementsForMonth()
    {
        var tenant = TestData.Tenant;
        var month = new ScheduledMonth(2020, 10);
        var requirements = new []
        {
            new UserRequirementModel("user1", "User 1", new Day(2022, 12, 2), "hard-no"), 
            new UserRequirementModel("user2", "User 2", new Day(2022, 12, 3), "soft-yes"), 
        };
        
        _requirementsServiceMock
            .Setup(_ => _.GetRequirementsOverviewForMonth())
            .ReturnsAsync(requirements);

        var controller = CreateController();

        var response = await controller.Get(tenant.Name, month.Year, month.Month);
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(requirements, result.Value);
    }

    private RequirementsController CreateController()
    {
        return new RequirementsController(_requirementsServiceMock.Object);
    }
}
