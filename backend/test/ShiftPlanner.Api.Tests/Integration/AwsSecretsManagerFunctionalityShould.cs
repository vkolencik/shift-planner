using Amazon;
using Amazon.SecretsManager;
using ShiftPlanner.Api.Services.Secrets;

namespace ShiftPlanner.Api.Tests.Integration;

public class AwsSecretsManagerFunctionalityShould
{
    [Fact]
    public async Task FetchSecret()
    {
        /* Secret payload:
                {"testPropertyString":"foo","testPropertyNumber":27,"testPropertyBoolean":true}
         */

        var client = new AwsSecretsManagerClient(new AmazonSecretsManagerClient(RegionEndpoint.EUCentral1));

        var secret = await client
            .FetchSecret<DummySecret>("arn:aws:secretsmanager:eu-central-1:720500131615:secret:test-secret-for-integration-tests-cVDrui");

        Assert.NotNull(secret);
        Assert.Equal("foo", secret.TestPropertyString);
        Assert.Equal(27, secret.TestPropertyNumber);
        Assert.True(secret.TestPropertyBoolean);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record DummySecret(string TestPropertyString, int TestPropertyNumber, bool TestPropertyBoolean);
}
