using Amazon.DynamoDBv2;
using Amazon.SecretsManager;
using Microsoft.Extensions.Options;
using ShiftPlanner.Api;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Api.Services.Secrets;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.GoogleSheet;

namespace ShiftPlanner.ScheduleCalculator;

static class Program
{
    public static async Task Main(string[] args)
    {
        var auth0UserService = new Auth0UserService(new Auth0TokenProvider(new HttpClientFactory()), new AwsSecretsManagerClient(new AmazonSecretsManagerClient()));

        ShiftAssignmentByEmail[] lastMonthSchedule = new[]
        {
            new ShiftAssignmentByEmail("jan.drasnar@bulovka.cz", new Day(2022, 12, 30), Workplace.A1),
            new ShiftAssignmentByEmail("daniela.fischer@bulovka.cz", new Day(2022, 12, 30), Workplace.A2),
            new ShiftAssignmentByEmail("juliana.paulovicova@bulovka.cz", new Day(2022, 12, 30), Workplace.A3),
            new ShiftAssignmentByEmail("zdenka.sraierova@bulovka.cz", new Day(2022, 12, 30), Workplace.Res),
            new ShiftAssignmentByEmail("katerina.michalovic@bulovka.cz", new Day(2022, 12, 30), Workplace.Kpr),
            new ShiftAssignmentByEmail("pavol.fedurcak@bulovka.cz", new Day(2022, 12, 30), Workplace.Urg),

            new ShiftAssignmentByEmail("petr.jiricka@bulovka.cz", new Day(2022, 12, 31), Workplace.A1),
            new ShiftAssignmentByEmail("daniel.dzunda@bulovka.cz", new Day(2022, 12, 31), Workplace.A2),
            new ShiftAssignmentByEmail("rafael.sepp@bulovka.cz", new Day(2022, 12, 31), Workplace.A3),
            new ShiftAssignmentByEmail("jan.tesarik@bulovka.cz", new Day(2022, 12, 31), Workplace.Res),
            new ShiftAssignmentByEmail("marcela.sumanova@bulovka.cz", new Day(2022, 12, 31), Workplace.Kpr),
            new ShiftAssignmentByEmail("simon.polka@bulovka.cz", new Day(2022, 12, 31), Workplace.Urg),

            new ShiftAssignmentByEmail("jan.drasnar@bulovka.cz", new Day(2023, 1, 1), Workplace.A1),
            new ShiftAssignmentByEmail("daniela.fischer@bulovka.cz", new Day(2023, 1, 1), Workplace.A2),
            new ShiftAssignmentByEmail("juliana.paulovicova@bulovka.cz", new Day(2023, 1, 1), Workplace.A3),
            new ShiftAssignmentByEmail("zdenka.sraierova@bulovka.cz", new Day(2023, 1, 1), Workplace.Res),
            new ShiftAssignmentByEmail("pavol.fedurcak@bulovka.cz", new Day(2023, 1, 1), Workplace.Kpr),
            new ShiftAssignmentByEmail("katerina.michalovic@bulovka.cz", new Day(2023, 1, 1), Workplace.Urg),
        };

        var aggregator = new SchedulingParametersAggregator(
            new GoogleSheetsEmployeeInfoService(),
            new RequirementsService(
                new DynamoRequirementsRepository(
                    new AmazonDynamoDBClient(),
                    new RequirementEntityConverter(),
                    Options.Create(new DynamoOptions() {TableNamePrefix = ""})),
                auth0UserService),
            auth0UserService,
            new StoredScheduleService(new GoogleSheetsEmployeeInfoService(), lastMonthSchedule));

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

class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}
