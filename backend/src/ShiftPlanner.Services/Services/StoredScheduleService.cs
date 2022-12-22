using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Services;

internal class StoredScheduleService : IStoredScheduleService
{
    private readonly IEmployeeInfoService _employeeInfoService;
    private readonly IScheduleRepository _scheduleRepository;

    private readonly IReadOnlyList<ShiftAssignmentByEmail> _lastMonthSchedule = new[]
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
    
    public StoredScheduleService(IEmployeeInfoService employeeInfoService, IScheduleRepository scheduleRepository)
    {
        _employeeInfoService = employeeInfoService;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Schedule?> GetSchedule(ScheduledMonth month)
    {
        // if (month == new ScheduledMonth(2022, 12))
        // {
        //     var employeeInfos = (await _employeeInfoService.GetEmployees(Workplace.Workplaces)).ToArray();
        //
        //     return new Schedule(_lastMonthSchedule.Select(sbe => GetScheduledShiftShift(sbe, employeeInfos)).ToArray());
        // }

        return await _scheduleRepository.LoadSchedule(month);

    }

    private ScheduledShift GetScheduledShiftShift(ShiftAssignmentByEmail assignmentByEmail, IReadOnlyList<EmployeeInfo> employeeInfos)
    {
        var employeeInfo = employeeInfos.Single(ei => ei.Email == assignmentByEmail.Email);
        return new ScheduledShift(assignmentByEmail.Day, assignmentByEmail.Workplace, new User("<blank>", employeeInfo.Name, employeeInfo.Email));
    } 
}

internal record ShiftAssignmentByEmail(string Email, Day Day, Workplace Workplace);
