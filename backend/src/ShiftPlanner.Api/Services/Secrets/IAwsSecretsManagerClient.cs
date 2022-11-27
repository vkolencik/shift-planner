namespace ShiftPlanner.Api.Services.Secrets;

public interface IAwsSecretsManagerClient
{
    Task<T> FetchSecret<T>(string secretArn);
}
