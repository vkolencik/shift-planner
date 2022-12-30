namespace ShiftPlanner.Contracts.Services;

public interface IAwsSecretsManagerClient
{
    Task<T> FetchSecret<T>(string secretArn);
    Task<string> FetchRawSecret(string secretArn);
}
