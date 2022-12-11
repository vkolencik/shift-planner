using Amazon;
using Amazon.SecretsManager;
using ShiftPlanner.Api.Services.Secrets;
using ShiftPlanner.Api.Services.Users;

namespace ShiftPlanner.Api.Tests.Integration;

public class Auth0TokenProviderShould
{
    [Fact]
    public async Task FetchToken()
    {
        var credentials = await new AwsSecretsManagerClient(new AmazonSecretsManagerClient(RegionEndpoint.EUCentral1))
            .FetchSecret<ClientCredentials>("arn:aws:secretsmanager:eu-central-1:720500131615:secret:auth0-mgmt-api-key-vdXHOP");
        var provider = new Auth0TokenProvider(new TestHttpClientFactory());

        var token = await provider.GetToken(credentials);

        Assert.NotEmpty(token);
    }
}
