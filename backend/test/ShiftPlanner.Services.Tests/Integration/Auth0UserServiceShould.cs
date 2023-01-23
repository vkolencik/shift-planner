using Amazon;
using Amazon.SecretsManager;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Infra;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Integration;

public class Auth0UserServiceShould
{
    [Fact]
    public async Task ReturnAllUsers()
    {
        var service = new Auth0UserService(new Auth0TokenProvider(new TestHttpClientFactory()), new AwsSecretsManagerClient(new AmazonSecretsManagerClient(RegionEndpoint.EUCentral1)));
        
        var users = await service.GetAllUsers();

        var expectedUser = new User("auth0|633015ee115794a47e8166d8", "Test User 1", "test_user_1@krtek.me");
        Assert.Contains(expectedUser, users);
    }
}
