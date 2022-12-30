using System.Text;
using System.Text.Json;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public class Auth0TokenProvider : IAuth0TokenProvider
{
    private IHttpClientFactory _httpClientFactory;

    public Auth0TokenProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetToken(ClientCredentials credentials)
    {
        var client = _httpClientFactory.CreateClient();
        
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
        if (token is null)
        {
            throw new InvalidOperationException("Couldn't get access_token from auth response");
        }

        return token;
    }
}
