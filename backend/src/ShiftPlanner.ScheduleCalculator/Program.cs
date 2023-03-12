using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
using ShiftPlanner.Rules;
using ShiftPlanner.Services;
using ShiftPlanner.Services.Persistence;
using ShiftPlanner.Services.Services;
using ShiftPlanner.Services.Services.Google;

namespace ShiftPlanner.ScheduleCalculator;

static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();

        var aggregator = services.GetRequiredService<ISchedulingParametersAggregator>();

        var month = new ScheduledMonth(2023, 4);
        var schedulingParams = await aggregator.GetSchedulingParameters(new Tenant("fnb"), month);

        var scheduler = new ShiftScheduler(new SequenceFlexibilityVariator(new ScheduleFlexibility(50)), new RulesetFactory());

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
            IScheduleRepository resultRepositoryGoogleSheet = services.GetRequiredService<IGoogleSheetsScheduleRepository>();
            IScheduleRepository resultRepositoryS3 = services.GetRequiredService<IScheduleRepository>();
            Console.Out.WriteLine("Solution found");
            Console.Out.WriteLine($"Flexibility: {successResult.Flexibility.Degree}");

            await resultRepositoryGoogleSheet.StoreComputedSchedule(month, successResult);
            await resultRepositoryS3.StoreComputedSchedule(month, successResult);
        }

        // Console.Out.WriteLine(string.Join('\n', result.SolutionParameters.RecordedStandards.Select(kvp => $"{kvp.Key.Name}: {kvp.Value.Minimum}-{kvp.Value.Maximum}")));
    }
}
