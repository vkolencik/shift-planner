using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Contracts.Services;

namespace ShiftPlanner.GoogleSheet;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleSheets(this IServiceCollection services)
    {
        services.AddTransient<IEmployeeInfoService, GoogleSheetsEmployeeInfoService>();
        services.AddTransient<IGoogleSheetsScheduleRepository, GoogleSheetsScheduleRepository>();
        return services;
    }
}
