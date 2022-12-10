using ShiftPlanner.Domain;
using ShiftPlanner.Domain.Services;

namespace ShiftPlanner.ScheduleCalculator;

class StoredScheduleService : IStoredScheduleService
{
    private readonly IEmployeeInfoService _employeeInfoService;

    private readonly IReadOnlyList<ShiftAssignmentByEmail> _lastMonthSchedule = new[]
    {
        new ShiftAssignmentByEmail("jan.drasnar@bulovka.cz", new Day(2022, 12, 30), Workplace.A1),
        new ShiftAssignmentByEmail("daniela.fischer@bulovka.cz", new Day(2022, 12, 30), Workplace.A2),
        new ShiftAssignmentByEmail("juliana.paulovicova@bulovka.cz", new Day(2022, 12, 30), Workplace.A3),
        new ShiftAssignmentByEmail("zdenka.sraierova@bulovka.cz", new Day(2022, 12, 30), Workplace.Res),
        new ShiftAssignmentByEmail("katerina.michalovic@bulovka.cz", new Day(2022, 12, 30), Workplace.Kpr),
        new ShiftAssignmentByEmail("pavol.fedurcak@bulovka.cz", new Day(2022, 12, 30), Workplace.Urg),
        
        new ShiftAssignmentByEmail("petr.jiricka@bulovka.cz", new Day(2022, 12, 30), Workplace.A1),
        new ShiftAssignmentByEmail("daniel.dzunda@bulovka.cz", new Day(2022, 12, 30), Workplace.A2),
        new ShiftAssignmentByEmail("rafael.sepp@bulovka.cz", new Day(2022, 12, 30), Workplace.A3),
        new ShiftAssignmentByEmail("jan.tesarik@bulovka.cz", new Day(2022, 12, 30), Workplace.Res),
        new ShiftAssignmentByEmail("marcela.sumanova@bulovka.cz", new Day(2022, 12, 30), Workplace.Kpr),
        new ShiftAssignmentByEmail("simon.polka@bulovka.cz", new Day(2022, 12, 30), Workplace.Urg)
    };
    
    public StoredScheduleService(IEmployeeInfoService employeeInfoService)
    {
        _employeeInfoService = employeeInfoService;
    }

    public async Task<Schedule> GetSchedule(ScheduledMonth month)
    {
        if (month != new ScheduledMonth(2022, 12))
        {
            throw new NotImplementedException();
        }
        
        var employeeInfos = (await _employeeInfoService.GetEmployees(Workplace.Workplaces)).ToArray();

        return Schedule.CreateSchedule(_lastMonthSchedule.Select(sbe => GetShiftAssignment(sbe, employeeInfos)));
    }

    private ShiftAssignment GetShiftAssignment(ShiftAssignmentByEmail assignmentByEmail, IReadOnlyList<EmployeeInfo> employeeInfos)
    {
        var employeeInfo = employeeInfos.Single(ei => ei.Email == assignmentByEmail.Email);
        var employee = new Employee(employeeInfo.Name, employeeInfo.CompetenceLevel, employeeInfo.Capabilities);
        return new ShiftAssignment(employee, assignmentByEmail.Day, assignmentByEmail.Workplace);
    } 
}

internal record ShiftAssignmentByEmail(string Email, Day Day, Workplace Workplace);
