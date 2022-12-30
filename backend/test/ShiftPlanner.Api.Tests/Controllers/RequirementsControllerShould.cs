using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

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
        var month = new ScheduledMonth(2023, 6);
        var requirements = new []
        {
            new UserRequirementModel("user1", "User 1", new Day(month, 2), "hard-no", null), 
            new UserRequirementModel("user2", "User 2", new Day(month, 3), "soft-yes", null), 
            new UserRequirementModel("user2", "User 2", new Day(month, 4), "leave", true), 
        };

        var expectedOutputModel = new []
        {
            new UserRequirementOutputModel("user1", "User 1", "2023-06-02", "hard-no", null), 
            new UserRequirementOutputModel("user2", "User 2", "2023-06-03", "soft-yes", null), 
            new UserRequirementOutputModel("user2", "User 2", "2023-06-04", "leave", true), 
        };
        
        _requirementsServiceMock
            .Setup(_ => _.GetRequirementsOverviewForMonth(tenant, month))
            .ReturnsAsync(requirements);

        var controller = CreateController();

        var response = await controller.Get(tenant.Name, month.Year, month.Month);
        
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(expectedOutputModel, result.Value);
    }

    private RequirementsController CreateController()
    {
        return new RequirementsController(_requirementsServiceMock.Object);
    }
}
