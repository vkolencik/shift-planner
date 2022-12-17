using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.Domain.Services;
using ShiftPlanner.Services;

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

    public async Task<StoredSchedule> GetSchedule(ScheduledMonth month)
    {
        if (month != new ScheduledMonth(2022, 12))
        {
            throw new NotImplementedException();
        }
        
        var employeeInfos = (await _employeeInfoService.GetEmployees(Workplace.Workplaces)).ToArray();

        return new StoredSchedule(_lastMonthSchedule.Select(sbe => GetScheduledShiftShift(sbe, employeeInfos)).ToArray());
    }

    private ScheduledShift GetScheduledShiftShift(ShiftAssignmentByEmail assignmentByEmail, IReadOnlyList<EmployeeInfo> employeeInfos)
    {
        var employeeInfo = employeeInfos.Single(ei => ei.Email == assignmentByEmail.Email);
        return new ScheduledShift(assignmentByEmail.Day, assignmentByEmail.Workplace, new User("<blank>", employeeInfo.Name, employeeInfo.Email));
    } 
}

internal record ShiftAssignmentByEmail(string Email, Day Day, Workplace Workplace);
