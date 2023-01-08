using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SecretsManager;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Services.Infra;
using ShiftPlanner.Services.Persistence;
using ShiftPlanner.Services.Services;
using ShiftPlanner.Services.Services.Google;

namespace ShiftPlanner.Services;

public static class ServiceCollectionExtensions
{
    private static readonly RegionEndpoint AwsRegionEndpoint = RegionEndpoint.EUCentral1;
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddAWSService<IAmazonS3>(new AWSOptions() {Region = AwsRegionEndpoint});
        services.AddAWSService<IAmazonDynamoDB>(new AWSOptions() {Region = AwsRegionEndpoint});
        services.AddAWSService<IAmazonSecretsManager>(new AWSOptions() {Region = AwsRegionEndpoint});
        services.AddSingleton<IAwsSecretsManagerClient, AwsSecretsManagerClient>();
        services.AddSingleton<IAuth0TokenProvider, Auth0TokenProvider>();
        
        services.AddTransient<IRequirementEntityConverter, RequirementEntityConverter>();
        services.AddTransient<IRequirementsRepository, DynamoRequirementsRepository>();
        services.AddTransient<IScheduleRepository, S3ScheduleRepository>();

        services.AddSingleton<IUserService, Auth0UserService>();
        services.AddTransient<IStoredScheduleService, StoredScheduleService>();
        services.AddTransient<IRequirementsService, RequirementsService>();
        services.AddTransient<IScheduleValidator, ScheduleValidator>();

        services.AddTransient<IEmployeeInfoService, GoogleSheetsEmployeeInfoService>();
        services.AddTransient<IGoogleSheetsScheduleRepository, GoogleSheetsScheduleRepository>();
        
        services.AddTransient<ISchedulingParametersAggregator, SchedulingParametersAggregator>();
        
        return services;
    }
}
