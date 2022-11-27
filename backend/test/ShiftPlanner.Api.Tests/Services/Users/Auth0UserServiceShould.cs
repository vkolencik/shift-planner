using ShiftPlanner.Api.Services.Users;

namespace ShiftPlanner.Api.Tests.Services.Users;

public class Auth0UserServiceShould
{
    [Fact]
    public async Task ReturnAllUsers()
    {
        var service = new Auth0UserService();
        service.GetAllUsers();
    }
}
