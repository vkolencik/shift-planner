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

        Console.Out.WriteLine("Fetching parameters...");
        var month = new ScheduledMonth(2023, 9);
        var schedulingParams = await aggregator.GetSchedulingParameters(new Tenant("fnb"), month);

        Validate(schedulingParams);

        Console.Out.WriteLine("Initiating scheduler...");
        
        var scheduler = new ShiftScheduler(new SequenceFlexibilityVariator(new ScheduleFlexibility(50)), new RulesetFactory());
        // var scheduler = new ShiftScheduler(new SingleFlexibilityVariator(new ScheduleFlexibility(50)), new RulesetFactory());

        Console.Out.WriteLine("Scheduler ready, computing schedule...");

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

            // await resultRepositoryGoogleSheet.StoreComputedSchedule(month, successResult);
            // await resultRepositoryS3.StoreComputedSchedule(month, successResult);
        }

        // Console.Out.WriteLine(string.Join('\n', result.SolutionParameters.RecordedStandards.Select(kvp => $"{kvp.Key.Name}: {kvp.Value.Minimum}-{kvp.Value.Maximum}")));
    }

    private static void Validate(MonthScheduleParameters p)
    {
        var errors = new List<string>();
        
        foreach (var d in p.Days)
        {
            foreach (var w in p.Workplaces)
            {
                var employees = p.EmployeeMonths
                    .Where(em => em.Employee.Capabilities.AllowedWorkplaces.Contains(w))
                    .Where(em => em.Leaves.All(l => !l.Days.Contains(d)));
                
                var c = employees
                    .Count();

                if (c <= 2) errors.Add("" + d + "-" + w + "-" + string.Join("|", employees.Select(e => e.Employee.Name)));
            }
        }

        if (errors.Any())
        {
            // throw new Exception(string.Join("\n", errors));
            Console.Out.WriteLine(string.Join("\n", errors));
        }
            
    }
}
