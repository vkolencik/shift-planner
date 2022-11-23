using System.Text.Json;

namespace ShiftPlanner.Aws;

using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

public class FetchAwsSecret
{
/*
*	Use this code snippet in your app.
*	If you need more information about configurations or implementing the sample code, visit the AWS docs:
*	https://aws.amazon.com/developer/language/net/getting-started
*/


    public static async Task<ClientCredentials> GetSecret()
    {
        string secretName = "auth0-mgmt-api-key";
        string region = "eu-central-1";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
        };

        GetSecretValueResponse response;

        try
        {
            response = await client.GetSecretValueAsync(request);
        }
        catch (Exception e)
        {
            // For a list of the exceptions thrown, see
            // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
            throw e;
        }

        string secretJson = response.SecretString;

        var jsonDocument = JsonDocument.Parse(secretJson);
        var clientId = jsonDocument.RootElement.GetProperty("client_id").GetString();
        var clientSecret = jsonDocument.RootElement.GetProperty("client_secret").GetString();

        return new ClientCredentials(
            clientId ?? throw new InvalidOperationException(),
            clientSecret ?? throw new InvalidOperationException());
    }
}
