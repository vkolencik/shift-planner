using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SecretsManager;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Services.Persistence;
using ShiftPlanner.Services.Services.Google;

namespace ShiftPlanner.Services;

public static class ServiceCollectionExtensions
{
    private static readonly RegionEndpoint AwsRegionEndpoint = RegionEndpoint.EUCentral1;
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddAWSService<IAmazonS3>(new AWSOptions() {Region = AwsRegionEndpoint});
        services.AddAWSService<IAmazonDynamoDB>(new AWSOptions() {Region = AwsRegionEndpoint});
        services.AddAWSService<IAmazonSecretsManager>(new AWSOptions() {Region = AwsRegionEndpoint});
        services.AddTransient<IStoredScheduleService, StoredScheduleService>();
        services.AddTransient<IScheduleRepository, S3ScheduleRepository>();

        services.AddTransient<IEmployeeInfoService, GoogleSheetsEmployeeInfoService>();
        services.AddTransient<IGoogleSheetsScheduleRepository, GoogleSheetsScheduleRepository>();
        
        return services;
    }
}
