namespace ShiftPlanner.Auth0.Tests;

public class Auth0ClientShould
{
    [Fact]
    public async Task GetAllUsers()
    {
        var user = await Auth0ManagementClient.GetUser();
    }
}
