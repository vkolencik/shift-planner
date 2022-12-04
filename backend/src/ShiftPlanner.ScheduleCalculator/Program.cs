// See https://aka.ms/new-console-template for more information

using Amazon.DynamoDBv2;
using Amazon.SecretsManager;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ShiftPlanner.Api;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Api.Services.Secrets;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.GoogleSheet;
using ShiftPlanner.ScheduleCalculator;

class Program
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
                auth0UserService);

        var schedulingParams = await aggregator.GetSchedulingParameters(new Tenant("fnb"), new ScheduledMonth(2023, 1));
        
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

            Console.Out.WriteLine("Solution found");
            Console.Out.WriteLine($"Flexibility: {successResult.Flexibility.Degree}");
            Console.Out.WriteLine(JsonConvert.SerializeObject(successResult.Schedule.Shifts));
        }
    }
}

class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}
