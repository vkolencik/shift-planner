using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.GoogleSheet;

namespace ShiftPlanner.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IStoredScheduleService, StoredScheduleService>();
        services.AddGoogleSheets();
        return services;
    }
}
