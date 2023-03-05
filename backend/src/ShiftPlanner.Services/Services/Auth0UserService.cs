using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Microsoft.Extensions.Logging;
using ShiftPlanner.Services.Contracts;
using User = ShiftPlanner.Services.Contracts.User;

namespace ShiftPlanner.Services.Services;

public class Auth0UserService : IUserService
{
    private readonly ILogger<Auth0UserService> _logger;
    private readonly IAuth0TokenProvider _auth0TokenProvider;
    private readonly IAwsSecretsManagerClient _secretsManagerClient;

    public Auth0UserService(IAuth0TokenProvider auth0TokenProvider, IAwsSecretsManagerClient secretsManagerClient, ILogger<Auth0UserService> logger)
    {
        _auth0TokenProvider = auth0TokenProvider;
        _secretsManagerClient = secretsManagerClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<User>> GetAllUsers()
    {
        _logger.LogInformation("Fetching AWS credentials");
        var auth0ApiKey = await _secretsManagerClient.FetchSecret<ClientCredentials>("arn:aws:secretsmanager:eu-central-1:720500131615:secret:auth0-mgmt-api-key-vdXHOP");
        _logger.LogInformation("Getting Auth0 token");
        var token = await _auth0TokenProvider.GetToken(auth0ApiKey);
        
        _logger.LogInformation("Reading user list from Auth0 API");
        var mgmtClient = new ManagementApiClient(token, new Uri("https://krtek.eu.auth0.com/api/v2"));
        var users = await mgmtClient.Users.GetAllAsync(new GetUsersRequest{}, new PaginationInfo(0, 100));
        _logger.LogInformation("Fetched {} users", users.Count);

        return users.Select(u => new User(u.UserId, u.FullName, u.Email)).ToArray();
    }
}
