namespace ShiftPlanner.Contracts.Services;

public interface IAwsSecretsManagerClient
{
    Task<T> FetchSecret<T>(string secretArn);
}
