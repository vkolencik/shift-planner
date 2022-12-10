// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2;
using Amazon.SecretsManager;
using Microsoft.Extensions.Options;
using ShiftPlanner.Api;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Api.Services.Secrets;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Domain;
using ShiftPlanner.Domain.Services;
using ShiftPlanner.Engine;
using ShiftPlanner.GoogleSheet;

namespace ShiftPlanner.ScheduleCalculator;

static class Program
{
    public static async Task Main(string[] args)
    {
        var auth0UserService = new Auth0UserService(new Auth0TokenProvider(new HttpClientFactory()), new AwsSecretsManagerClient(new AmazonSecretsManagerClient()));
        
        var aggregator = new SchedulingParametersAggregator(
            new GoogleSheetsEmployeeInfoService(),
            new RequirementsService(
                new DynamoRequirementsRepository(
                    new AmazonDynamoDBClient(),
                    new RequirementEntityConverter(),
                    Options.Create(new DynamoOptions() {TableNamePrefix = ""})),
                auth0UserService),
            auth0UserService,
            new StoredScheduleService(new GoogleSheetsEmployeeInfoService()));

        var month = new ScheduledMonth(2023, 1);
        var schedulingParams = await aggregator.GetSchedulingParameters(new Tenant("fnb"), month);
        
        var scheduler = new ShiftScheduler();

        var result = scheduler.ComputeSchedule(schedulingParams);

        if (result == null)
        {
            throw new InvalidOperationException("Unable to compute schedule");
        }

        if (result is SolutionNotFound)
        {
            Console.Error.WriteLine("Solution not found");
        }

        if (result is SuccessfulSchedulingResult)
        {
            var successResult = (SuccessfulSchedulingResult) result;

            IScheduleRepository resultRepository = new GoogleSheetsScheduleRepository();
            Console.Out.WriteLine("Solution found");
            Console.Out.WriteLine($"Flexibility: {successResult.Flexibility.Degree}");
            
            await resultRepository.StoreComputedSchedule(month, successResult);
        }
    }
}

class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}
