using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests;

public class ServiceCollectionExtensionsShould
{
    [Fact]
    public void RegisterAllServices()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddServices();

        var provider = serviceCollection.BuildServiceProvider();

        provider.GetRequiredService<IUserService>();
        provider.GetRequiredService<IScheduleValidator>();
    }
}
