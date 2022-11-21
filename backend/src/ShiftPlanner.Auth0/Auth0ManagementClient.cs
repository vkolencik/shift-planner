using System.Text;
using Auth0.ManagementApi;
using System.Text.Json;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using ShiftPlanner.Aws;

namespace ShiftPlanner.Auth0;

public class Auth0ManagementClient
{
    public static async Task<User> GetUser()
    {
        var credentials = await FetchAwsSecret.GetSecret();
        
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
}
