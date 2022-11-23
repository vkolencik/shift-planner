using System.Net;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Moq;

namespace ShiftPlanner.Aws.Integration.Tests;

public class AwsSecretsManagerClientShould
{
    [Fact]
    public async Task Throw_WhenClientReturnsNokStatus()
    {
        var awsClientMock = new Mock<IAmazonSecretsManager>();
        awsClientMock
            .Setup(_ => _.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetSecretValueResponse() {HttpStatusCode = HttpStatusCode.InternalServerError});
        
        var client = new AwsSecretsManagerClient(awsClientMock.Object);
        
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.FetchSecret<string>("some-arn"));
    }
    
}
