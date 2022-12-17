using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.GoogleSheet;
using ShiftPlanner.ScheduleCalculator;

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
