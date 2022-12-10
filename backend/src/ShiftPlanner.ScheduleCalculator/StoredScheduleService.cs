using ShiftPlanner.Domain;
using ShiftPlanner.Domain.Services;

namespace ShiftPlanner.ScheduleCalculator;

class StoredScheduleService : IStoredScheduleService
{
    private readonly IEmployeeInfoService _employeeInfoService;

    private readonly IReadOnlyList<ShiftAssignmentByEmail> _lastMonthSchedule;
    
    public StoredScheduleService(IEmployeeInfoService employeeInfoService, IReadOnlyList<ShiftAssignmentByEmail> lastMonthSchedule)
    {
        _employeeInfoService = employeeInfoService;
        _lastMonthSchedule = lastMonthSchedule;
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
