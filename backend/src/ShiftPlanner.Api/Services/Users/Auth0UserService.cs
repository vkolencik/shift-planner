using System.Text.Json;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using ShiftPlanner.Api.Services.Secrets;
using User = ShiftPlanner.Api.Contracts.User;

namespace ShiftPlanner.Api.Services.Users;

internal class Auth0UserService : IUserService
{
    private readonly IAuth0TokenProvider _auth0TokenProvider;
    private readonly IAwsSecretsManagerClient _secretsManagerClient;

    public Auth0UserService(IAuth0TokenProvider auth0TokenProvider, IAwsSecretsManagerClient secretsManagerClient)
    {
        _auth0TokenProvider = auth0TokenProvider;
        _secretsManagerClient = secretsManagerClient;
    }

    /*    public static async Task<User> GetUser()
    {
        var credentials = await AwsSecretsManagerClient.GetSecret();
        
        var client = new HttpClient();
        var payload = JsonSerializer.Serialize(
            new
            {
                client_id = credentials.ClientId,
                client_secret = credentials.ClientSecret,
                audience = "https://krtek.eu.auth0.com/api/v2/",
                grant_type = "client_credentials"
            });

        var response = await client.PostAsync("https://krtek.eu.auth0.com/oauth/token", new StringContent(payload, Encoding.UTF8, "application/json"));
        var data = JsonDocument.Parse(response.Content.ReadAsStream());
        var token = data.RootElement.GetProperty("access_token").GetString();
            
        var mgmtClient = new ManagementApiClient(token, new Uri("https://krtek.eu.auth0.com/api/v2"));
        var users = await mgmtClient.Users.GetAllAsync(new GetUsersRequest{}, new PaginationInfo(0, 100));
        var tester = users.Where(u => u.Email == "e2e-test-user-1@krtek.me").Single();
        Console.Out.WriteLine(JsonSerializer.Serialize(tester));

        return tester;
    }
*/
    public async Task<IReadOnlyCollection<User>> GetAllUsers()
    {
        var auth0ApiKey = await _secretsManagerClient.FetchSecret<ClientCredentials>("arn:aws:secretsmanager:eu-central-1:720500131615:secret:auth0-mgmt-api-key-vdXHOP");
        var token = await _auth0TokenProvider.GetToken(auth0ApiKey);
        
        var mgmtClient = new ManagementApiClient(token, new Uri("https://krtek.eu.auth0.com/api/v2"));
        var users = await mgmtClient.Users.GetAllAsync(new GetUsersRequest{}, new PaginationInfo(0, 100));

        return users.Select(u => new User(u.UserId, u.FullName, u.Email)).ToArray();
    }
}
