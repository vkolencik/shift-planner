using Amazon;
using Amazon.SecretsManager;
using ShiftPlanner.Services.Infra;

namespace ShiftPlanner.Api.Tests.Integration;

public class AwsSecretsManagerFunctionalityShould
{
    /* Secret payload:
            {"testPropertyString":"foo","testPropertyNumber":27,"testPropertyBoolean":true}
     */
    private static readonly string TestSecretArn = "arn:aws:secretsmanager:eu-central-1:720500131615:secret:test-secret-for-integration-tests-cVDrui";

    [Fact]
    public async Task FetchSecret()
    {

        var client = new AwsSecretsManagerClient(new AmazonSecretsManagerClient(RegionEndpoint.EUCentral1));

        var secret = await client
            .FetchSecret<DummySecret>(TestSecretArn);

        Assert.NotNull(secret);
        Assert.Equal("foo", secret.TestPropertyString);
        Assert.Equal(27, secret.TestPropertyNumber);
        Assert.True(secret.TestPropertyBoolean);
    }

    [Fact]
    public async Task FetchRawSecret()
    {

        var client = new AwsSecretsManagerClient(new AmazonSecretsManagerClient(RegionEndpoint.EUCentral1));

        var secret = await client.FetchRawSecret(TestSecretArn);

        Assert.Equal(@"{""testPropertyString"":""foo"",""testPropertyNumber"":27,""testPropertyBoolean"":true}", secret);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record DummySecret(string TestPropertyString, int TestPropertyNumber, bool TestPropertyBoolean);
}
