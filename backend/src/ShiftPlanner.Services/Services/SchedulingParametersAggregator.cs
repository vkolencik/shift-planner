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

        var activeEmployeeMonths = activeEmployeeInfos.Select(
                ei => new EmployeeMonth(
                    new Employee(ei.Name, ei.CompetenceLevel, ei.Capabilities, ei.Workload),
                    GetLeavesForEmployee(month, employeeRequirements, userIdsByEmail[ei.Email])
                ))
            .ToArray();

        var previousMonth = month.Previous;
        var existingShifts = (await _storedScheduleService.GetSchedule(previousMonth)).Shifts;

        var missingUsers = existingShifts.Select(s => s.User.Email).ToHashSet().Except(employeeInfos.Select(ei => ei.Email).ToHashSet()).ToArray();

        if (missingUsers.Any())
        {
            throw new InvalidOperationException($"Missing employee infos for {string.Join(", ", missingUsers)}");
        }

        var lastMonthShifts = existingShifts.Where(s => s.Day.Month == previousMonth.Month)
            .Select(ss => new ShiftAssignment(GetEmployeeFromInfo(employeeInfos.Single(ei => ei.Email == ss.User.Email)), ss.Day, ss.Workplace))
            .ToArray();

        var externistaInfo = employeeInfos.SingleOrDefault(ei => ei.Email == "externista1@bulovka.cz");
        var externista = new Employee(externistaInfo.Name, externistaInfo.CompetenceLevel, externistaInfo.Capabilities, externistaInfo.Workload);
        var predefinedShifts = existingShifts
            .Where(ss => ss.Day.Month == month.Month)
            .Select(ss => new ShiftAssignment(GetEmployeeFromInfo(employeeInfos.Single(ei => ei.Email == ss.User.Email)), ss.Day, ss.Workplace))
            .Concat(
                new ShiftAssignment[]
                {
                    new ShiftAssignment(externista, new Day(2023, 4, 4), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 6), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 11), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 18), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 25), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 26), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 16), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 22), Workplace.Urg),
                    new ShiftAssignment(externista, new Day(2023, 4, 23), Workplace.Urg), 
                })
            .ToArray();

        return new MonthScheduleParameters(
            month,
            new[] {externista},
            activeEmployeeMonths,
            CompetenceGroup.CompetenceGroups,
            lastMonthShifts,
            predefinedShifts,
            Workplace.WeekendInterchangeabilityGroups);
    }

    private Employee GetEmployeeFromInfo(EmployeeInfo ei)
    {
        return new Employee(ei.Name, ei.CompetenceLevel, ei.Capabilities, ei.Workload);
    }

    private IReadOnlyCollection<Leave> GetLeavesForEmployee(ScheduledMonth month, IReadOnlyCollection<UserRequirementModel> employeeRequirements, string userId) =>
        employeeRequirements
            .Where(req => req.UserId == userId)
            .Where(req => AbsenceRequirementTypes.Contains(req.RequirementType))
            .Select(req => new Leave(req.Date, req.Date, req.RequirementType == "leave"))
            .Concat(
                (userId == "auth0|637f84e6841043dd7a9d449e" // Tesarik
                 || userId == "auth0|637f84e6841043dd7a9d449f") // Tesarova
                    ? month.Days
                        .Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                        .Select(d => new Leave(d, d, false))
                    : Array.Empty<Leave>())
            .Concat(
                userId == "auth0|637f84e6841043dd7a9d44a0" // Touficova
                    ? month.Days
                        .Where(d => d.DayOfWeek == DayOfWeek.Friday || d.DayOfWeek == DayOfWeek.Sunday)
                        .Select(d => new Leave(d, d, false))
                    : Array.Empty<Leave>())
            .ToArray();
}
