using Amazon;
using Amazon.SecretsManager;
using Microsoft.Extensions.Logging.Abstractions;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Infra;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Integration;

public class Auth0UserServiceShould
{
    [Fact]
    public async Task ReturnAllUsers()
    {
        var service = new Auth0UserService(
            new Auth0TokenProvider(new TestHttpClientFactory()),
            new AwsSecretsManagerClient(new AmazonSecretsManagerClient(RegionEndpoint.EUCentral1)),
            NullLogger<Auth0UserService>.Instance);
        
        var users = await service.GetAllUsers();

        var expectedActiveUser = new User("auth0|633015ee115794a47e8166d8", "Test User 1", "test_user_1@krtek.me", User.MaxDate);
        var expectedDeactivatedUser = new User("auth0|64e321894601e446d813236e", "Deactivated User", "deactivated_user@krtek.me", new DateOnly(2023, 08, 21));
        Assert.Contains(expectedActiveUser, users);
        Assert.Contains(expectedDeactivatedUser, users);
    }
}
