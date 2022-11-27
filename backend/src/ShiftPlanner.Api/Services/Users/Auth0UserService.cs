using ShiftPlanner.Api.Contracts;

namespace ShiftPlanner.Api.Services.Users;

internal class Auth0UserService : IUserService
{
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
            
        Console.Out.WriteLine($"Token: {token}");
        
        var mgmtClient = new ManagementApiClient(token, new Uri("https://krtek.eu.auth0.com/api/v2"));
        var users = await mgmtClient.Users.GetAllAsync(new GetUsersRequest{}, new PaginationInfo(0, 100));
        var tester = users.Where(u => u.Email == "e2e-test-user-1@krtek.me").Single();
        Console.Out.WriteLine(JsonSerializer.Serialize(tester));

        return tester;
    }
*/
    public Task<IReadOnlyCollection<User>> GetAllUsers()
    {
        return Task.FromResult(
            (IReadOnlyCollection<User>) new[]
            {
                new User("auth0|633015ee115794a47e8166d8", "Test User 1", "test_user_1@krtek.me"),
                new User("auth0|6367b6f40df1e7bc1037834f", "Test User 2", "test_user_2@krtek.me"),
            });
    }
}
