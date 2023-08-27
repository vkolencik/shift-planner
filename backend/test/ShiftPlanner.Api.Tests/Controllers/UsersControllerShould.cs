using Microsoft.AspNetCore.Mvc;
using Moq;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Api.Tests.Controllers;

public class UsersControllerShould
{
    private Mock<IUserService> _userServiceMock;

    public UsersControllerShould()
    {
        _userServiceMock = new Mock<IUserService>();
    }

    [Fact]
    public async Task ReturnUser()
    {
        var expectedUsers = new []
        {
            new FullUserOutputModel("user1", "User 1", "2023-08")
        };
        _userServiceMock
            .Setup(_ => _.GetAllUsers())
            .ReturnsAsync(new[] {new User("user1", "User 1", "user1@krtek.me", new ScheduledMonth(2023, 08))});
        
        var usersController = new UsersController(_userServiceMock.Object);

        var response = await usersController.GetUsers();
        
        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.NotNull(result.Value);
        var users = (IEnumerable<FullUserOutputModel>) result.Value!;
        Assert.Equal(expectedUsers, users);
    }
}

