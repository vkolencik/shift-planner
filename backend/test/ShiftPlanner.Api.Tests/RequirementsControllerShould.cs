using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Api.Controllers;

namespace ShiftPlanner.Api.Tests;

public class RequirementsControllerShould
{
    [Fact]
    public void ReturnUnauthorized_GivenTokenForDifferentUser()
    {
        var controller = CreateController("user-id-1");

        var response = controller.Get("tenant-1", "user-id-2");
        
        Assert.IsType<UnauthorizedObjectResult>(response.Result);
    }

    private RequirementsController CreateController(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, userId),
        }, "mock"));

        var controller = new RequirementsController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        return controller;
    }
}
