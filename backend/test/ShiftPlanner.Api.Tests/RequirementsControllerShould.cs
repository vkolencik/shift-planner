using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Controllers;

namespace ShiftPlanner.Api.Tests;

public class RequirementsControllerShould
{
    private Mock<IRequirementsService> _requirementsServiceMock = new Mock<IRequirementsService>();

    [Fact]
    public async Task ReturnListOfRequirementsForAGivenMonth()
    {
        int year = 2022;
        int month = 10;
        string userId = "user-id-1";
        var requirements = new Dictionary<string, string>
        {
            ["2022-10-01"] = "soft-yes",
            ["2022-12-13"] = "hard-no"
        };
        
        _requirementsServiceMock.Setup(_ => _.GetRequirements(userId, year, month)).ReturnsAsync(requirements);
        var controller = CreateController(userId);

        var response = await controller.Get("tenant-1", userId, year, month);
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(requirements, result.Value);
    }
    
    [Fact]
    public async Task ReturnUnauthorized_GivenTokenForDifferentUser()
    {
        var controller = CreateController("user-id-1");

        var response = await controller.Get("tenant-1", "user-id-2", 2022, 10);
        
        Assert.IsType<UnauthorizedObjectResult>(response.Result);
    }

    private RequirementsController CreateController(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, userId),
        }, "mock"));

        var controller = new RequirementsController(_requirementsServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() { User = user }
            }
        };

        return controller;
    }
}
