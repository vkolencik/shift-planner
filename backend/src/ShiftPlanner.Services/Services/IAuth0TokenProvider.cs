using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public interface IAuth0TokenProvider
{
    Task<string> GetToken(ClientCredentials credentials);
}
