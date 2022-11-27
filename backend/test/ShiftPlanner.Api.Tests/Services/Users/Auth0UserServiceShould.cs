using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Services.Users;

namespace ShiftPlanner.Api.Tests.Services.Users;

public class Auth0UserServiceShould
{
    [Fact]
    public async Task ReturnAllUsers()
    {
        var service = new Auth0UserService();
        
        var users = await service.GetAllUsers();

        var expectedUser = new User("user10", "User 10", "user10@krtek.me");
        Assert.Contains(expectedUser, users);
    }
}
