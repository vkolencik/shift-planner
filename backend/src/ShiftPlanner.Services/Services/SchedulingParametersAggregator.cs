using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

internal class SchedulingParametersAggregator : ISchedulingParametersAggregator
{
    private readonly IEmployeeInfoService _employeeInfoService;
    private readonly IRequirementsService _requirementsService;
    private readonly IUserService _userService;
    private readonly IStoredScheduleService _storedScheduleService;
    private static readonly HashSet<string> AbsenceRequirementTypes = new() {"leave", "hard-no", "only-morning"};

    public SchedulingParametersAggregator(
        IEmployeeInfoService employeeInfoService,
        IRequirementsService requirementsService,
        IUserService userService,
        IStoredScheduleService storedScheduleService)
    {
        _employeeInfoService = employeeInfoService;
        _requirementsService = requirementsService;
        _userService = userService;
        _storedScheduleService = storedScheduleService;
    }

    public async Task<MonthScheduleParameters> GetSchedulingParameters(Tenant tenant, ScheduledMonth month)
    {
        var employeeInfos = await _employeeInfoService.GetEmployees(Workplace.Workplaces);

        var activeEmployeeInfos = employeeInfos.Where(e => e.EmployeeStatus == EmployeeStatus.Active);

        var employeeRequirements = await _requirementsService.GetRequirementsOverviewForMonth(tenant, month);

        var users = await _userService.GetAllUsers();
        var userIdsByEmail = users.ToDictionary(u => u.Email, u => u.UserId);

        var employeeMonths = activeEmployeeInfos.Select(
                ei => new EmployeeMonth(
                    new Employee(ei.Name, ei.CompetenceLevel, ei.Capabilities, ei.Workload),
                    GetLeavesForEmployee(employeeRequirements, userIdsByEmail[ei.Email])
                ))
            .ToArray();

        var previousMonth = month.Previous;
        var existingShifts = (await _storedScheduleService.GetSchedule(previousMonth)).Shifts;

        var missingUsers = existingShifts.Select(s => s.User.Email).ToHashSet().Except(employeeInfos.Select(ei => ei.Email).ToHashSet()).ToArray();

        if (missingUsers.Any())
        {
            throw new InvalidOperationException($"Missing employee infos for {string.Join(", ", missingUsers)}");
        }
        
        var lastMonthShifts = existingShifts.
            Where(s => s.Day.Month == previousMonth.Month)
            .Select(ss => new ShiftAssignment(GetEmployeeFromInfo(employeeInfos.Single(ei => ei.Email == ss.User.Email)), ss.Day, ss.Workplace))
            .ToArray();
        
        var fixedShifts = existingShifts
            .Where(ss => ss.Day.Month == month.Month)
            .Select(ss => new ShiftAssignment(GetEmployeeFromInfo(employeeInfos.Single(ei => ei.Email == ss.User.Email)), ss.Day, ss.Workplace))
            .ToArray();
        
        return new MonthScheduleParameters(
            month,
            employeeMonths,
            CompetenceGroup.CompetenceGroups,
            lastMonthShifts,
            fixedShifts,
            Workplace.WeekendInterchangeabilityGroups);
    }

    private Employee GetEmployeeFromInfo(EmployeeInfo ei)
    {
        return new Employee(ei.Name, ei.CompetenceLevel, ei.Capabilities, ei.Workload);
    }
    
    private IReadOnlyCollection<Leave> GetLeavesForEmployee(IReadOnlyCollection<UserRequirementModel> employeeRequirements, string userId) =>
        employeeRequirements
            .Where(req => req.UserId == userId)
            .Where(req => AbsenceRequirementTypes.Contains(req.RequirementType))
            .Select(req => new Leave(req.Date, req.Date, req.RequirementType == "leave"))
            .ToArray();
}
