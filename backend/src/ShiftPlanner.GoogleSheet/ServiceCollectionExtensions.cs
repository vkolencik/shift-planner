using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Services;

namespace ShiftPlanner.GoogleSheet;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleSheets(this IServiceCollection services)
    {
        services.AddTransient<IEmployeeInfoService, GoogleSheetsEmployeeInfoService>();
        return services;
    }
}
