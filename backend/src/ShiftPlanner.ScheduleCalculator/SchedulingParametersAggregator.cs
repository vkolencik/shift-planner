using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Domain;
using ShiftPlanner.GoogleSheet;

namespace ShiftPlanner.ScheduleCalculator;

internal class SchedulingParametersAggregator : ISchedulingParametersAggregator
{
    private readonly IEmployeeInfoService _employeeInfoService;
    private readonly IRequirementsService _requirementsService;
    private readonly IUserService _userService;
    private static readonly HashSet<string> AbsenceRequirementTypes = new() {"leave", "hard-no", "only-morning"};

    public SchedulingParametersAggregator(IEmployeeInfoService employeeInfoService, IRequirementsService requirementsService, IUserService userService)
    {
        _employeeInfoService = employeeInfoService;
        _requirementsService = requirementsService;
        _userService = userService;
    }

    public async Task<MonthScheduleParameters> GetSchedulingParameters(Tenant tenant, ScheduledMonth month)
    {
        var employeeInfos = await _employeeInfoService.GetEmployees(Workplace.Workplaces);

        var employeeRequirements = await _requirementsService.GetRequirementsOverviewForMonth(tenant, month);

        var users = await _userService.GetAllUsers();
        var userIdsByEmail = users.ToDictionary(u => u.Email, u => u.UserId);

        var employeeMonths = employeeInfos.Select(
                ei => new EmployeeMonth(
                    new Employee(ei.Name, ei.CompetenceLevel, ei.Capabilities, ei.Workload),
                    GetLeavesForEmployee(employeeRequirements, userIdsByEmail[ei.Email])
                ))
            .ToArray();

        return new MonthScheduleParameters(
            month,
            employeeMonths,
            CompetenceGroup.CompetenceGroups);
    }

    private IReadOnlyCollection<Leave> GetLeavesForEmployee(IReadOnlyCollection<UserRequirementModel> employeeRequirements, string userId) =>
        employeeRequirements
            .Where(req => req.UserId == userId)
            .Where(req => AbsenceRequirementTypes.Contains(req.RequirementType))
            .Select(req => new Leave(req.Date, req.Date))
            .ToArray();
}
