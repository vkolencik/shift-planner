using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

public class S3ScheduleRepository : IScheduleRepository
{
    public Task StoreComputedSchedule(ScheduledMonth month, SuccessfulSchedulingResult result)
    {
        throw new NotImplementedException();
    }

    public Task<StoredSchedule> LoadLatestSchedule(ScheduledMonth month)
    {
        throw new NotImplementedException();
    }
}
