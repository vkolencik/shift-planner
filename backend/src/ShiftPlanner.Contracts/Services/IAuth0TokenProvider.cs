namespace ShiftPlanner.Contracts.Services;

public interface IAuth0TokenProvider
{
    Task<string> GetToken(ClientCredentials credentials);
}
