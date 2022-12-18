using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using User = ShiftPlanner.Contracts.User;

namespace ShiftPlanner.Services.Services;

public class Auth0UserService : IUserService
{
    private readonly IAuth0TokenProvider _auth0TokenProvider;
    private readonly IAwsSecretsManagerClient _secretsManagerClient;

    public Auth0UserService(IAuth0TokenProvider auth0TokenProvider, IAwsSecretsManagerClient secretsManagerClient)
    {
        _auth0TokenProvider = auth0TokenProvider;
        _secretsManagerClient = secretsManagerClient;
    }

    public async Task<IReadOnlyCollection<User>> GetAllUsers()
    {
        var auth0ApiKey = await _secretsManagerClient.FetchSecret<ClientCredentials>("arn:aws:secretsmanager:eu-central-1:720500131615:secret:auth0-mgmt-api-key-vdXHOP");
        var token = await _auth0TokenProvider.GetToken(auth0ApiKey);
        
        var mgmtClient = new ManagementApiClient(token, new Uri("https://krtek.eu.auth0.com/api/v2"));
        var users = await mgmtClient.Users.GetAllAsync(new GetUsersRequest{}, new PaginationInfo(0, 100));

        return Enumerable.ToArray<User>(users.Select(u => new User(u.UserId, u.FullName, u.Email)));
    }
}
