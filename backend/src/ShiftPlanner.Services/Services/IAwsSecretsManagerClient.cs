namespace ShiftPlanner.Services.Services;

public interface IAwsSecretsManagerClient
{
    Task<T> FetchSecret<T>(string secretArn);
    Task<string> FetchRawSecret(string secretArn);
}
