using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests.Persistence;

public class S3ScheduleRepositoryShould
{
    [Fact(Skip = "")]
    public async Task StoreSchedule()
    {
        var repository = CreateRepository();

        await repository.StoreComputedSchedule(
            new ScheduledMonth(2023, 1),
            new SuccessfulSchedulingResult(
                Schedule.CreateSchedule(new ShiftAssignment[] { }),
                ScheduleFlexibility.Strict,
                new SolutionParameters(new Dictionary<Employee, EmployeeStandard>())));
    }

    [Fact(Skip = "")]
    public async Task LoadLatestSchedule()
    {
        var repository = CreateRepository();

        var schedule = await repository.LoadLatestSchedule(new ScheduledMonth(2023, 1));
    }

    private static S3ScheduleRepository CreateRepository()
    {
        return new S3ScheduleRepository();
    }
}
