using System.Net;
using System.Text.Json;

namespace ShiftPlanner.Aws;

using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

public class AwsSecretsManagerClient
{
    private readonly IAmazonSecretsManager _client;

    public AwsSecretsManagerClient()
    {
        _client = new AmazonSecretsManagerClient();
    }

    internal AwsSecretsManagerClient(IAmazonSecretsManager client)
    {
        _client = client;
    }

    public async Task<T> FetchSecret<T>(string secretArn)
    {
        var request = new GetSecretValueRequest {SecretId = secretArn};

        var response = await _client
            .GetSecretValueAsync(request)
            .ConfigureAwait(false);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Received {response.HttpStatusCode} status code while fetching the secret {secretArn}");
        }

        var secretJson = response.SecretString;

        return JsonSerializer.Deserialize<T>(secretJson, new JsonSerializerOptions {PropertyNameCaseInsensitive = true}) ?? throw new InvalidOperationException($"Couldn't parse secret response JSON while fetching the secret {secretArn}");
    }
}
