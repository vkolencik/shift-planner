using Microsoft.Extensions.DependencyInjection;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;
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

        var month = new ScheduledMonth(2023, 2);
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
            IScheduleRepository resultRepository = services.GetRequiredService<IGoogleSheetsScheduleRepository>();
            Console.Out.WriteLine("Solution found");
            Console.Out.WriteLine($"Flexibility: {successResult.Flexibility.Degree}");

            await resultRepository.StoreComputedSchedule(month, successResult);
        }

        Console.Out.WriteLine(string.Join('\n', result.SolutionParameters.RecordedStandards.Select(kvp => $"{kvp.Key.Name}: {kvp.Value.Minimum}-{kvp.Value.Maximum}")));
    }
}
