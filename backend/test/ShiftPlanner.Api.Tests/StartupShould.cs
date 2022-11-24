using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShiftPlanner.Api.Controllers;

namespace ShiftPlanner.Api.Tests;

public class StartupShould
{
    [Fact]
    public void RegisterAllServices()
    {
        using MemoryStream ms = CreateConfigurationStream();

        IHostBuilder builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureServices(
                        serviceCollection => { serviceCollection.AddTransient<RequirementsController>(); });
                    webBuilder.ConfigureAppConfiguration(configurationBuilder => { configurationBuilder.AddJsonStream(ms); });
                });

        using IHost host = builder.Build();
        IServiceProvider services = host.Services;

        Assert.NotNull(services.GetService<RequirementsController>());
    }

    private static MemoryStream CreateConfigurationStream()
    {
        string s =
            "{\n  \"Auth0\": {\n    \"Domain\": \"krtek.eu.auth0.com\",\n    \"Audience\": \"https://b.krtek.me\"\n  }\n}";
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(s));
        return ms;
    }
}
