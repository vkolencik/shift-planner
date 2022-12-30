using ShiftPlanner.Services;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SecretsManager;
using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Infra;
using ShiftPlanner.Services.Services;
using ShiftPlanner.Services.Services.Google;

namespace ShiftPlanner.ScheduleCalculator;

static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddServices()
            .AddTransient<IRequirementsService, RequirementsService>()
            .AddTransient<IRequirementsRepository, DynamoRequirementsRepository>()
            .AddTransient<IRequirementEntityConverter, RequirementEntityConverter>()
            .AddHttpClient()
            .AddAWSService<IAmazonDynamoDB>(new AWSOptions() {Region = RegionEndpoint.EUCentral1})
            .AddAWSService<IAmazonSecretsManager>(new AWSOptions() {Region = RegionEndpoint.EUCentral1})
            .AddSingleton<IAwsSecretsManagerClient, AwsSecretsManagerClient>()
            .AddSingleton<IAuth0TokenProvider, Auth0TokenProvider>()
            .AddSingleton<IUserService, Auth0UserService>()
            .AddSingleton<SchedulingParametersAggregator>()
            .BuildServiceProvider();

        var aggregator = services.GetRequiredService<SchedulingParametersAggregator>();

        var month = new ScheduledMonth(2023, 1);
        var schedulingParams = await aggregator.GetSchedulingParameters(new Tenant("fnb"), month);

        var scheduler = new Engine.ShiftScheduler();

        var result = scheduler.ComputeSchedule(schedulingParams);

        if (result == null)
        {
            throw new InvalidOperationException("Unable to compute schedule");
        }

        if (result is SolutionNotFound)
        {
            Console.Error.WriteLine("Solution not found");
        }

        if (result is SuccessfulSchedulingResult successResult)
        {
            IScheduleRepository resultRepository = new GoogleSheetsScheduleRepository();
            Console.Out.WriteLine("Solution found");
            Console.Out.WriteLine($"Flexibility: {successResult.Flexibility.Degree}");

            await resultRepository.StoreComputedSchedule(month, successResult);
        }

        Console.Out.WriteLine(string.Join('\n', result.SolutionParameters.RecordedStandards.Select(kvp => $"{kvp.Key.Name}: {kvp.Value.Minimum}-{kvp.Value.Maximum}")));
    }
}
