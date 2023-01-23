using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Persistence;
using ShiftPlanner.Services.Services;

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

    public async Task StoreSchedule(Tenant tenant, ScheduledMonth month, Schedule schedule)
    {
        await _scheduleRepository.StoreSchedule(month, schedule);
    }
}
