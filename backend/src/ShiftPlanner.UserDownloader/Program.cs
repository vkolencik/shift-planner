// See https://aka.ms/new-console-template for more information

using System.IO;
using System.Net.Http;
using System.Text.Json;
using Amazon.SecretsManager;
using ShiftPlanner.Services.Services;

var service = new Auth0UserService(new Auth0TokenProvider(new HttpClientFactory()), new AwsSecretsManagerClient(new AmazonSecretsManagerClient()));

var users = service.GetAllUsers();

File.WriteAllText("users.json", JsonSerializer.Serialize(users));
    
class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}

