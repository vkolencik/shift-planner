using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Services;

internal class StoredScheduleService : IStoredScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;

    public StoredScheduleService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Schedule?> GetSchedule(ScheduledMonth month)
    {
        return await _scheduleRepository.LoadSchedule(month);
    }
}
