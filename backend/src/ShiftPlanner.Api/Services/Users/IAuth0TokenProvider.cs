using ShiftPlanner.Api.Services.Secrets;

namespace ShiftPlanner.Api.Services.Users;

public interface IAuth0TokenProvider
{
    Task<string> GetToken(ClientCredentials credentials);
}
