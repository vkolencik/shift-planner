namespace ShiftPlanner.Api.Tests.Integration;

public class TestHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}
