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

        var externalChirurgove = GetExternalEmployee(employeeInfos, "externista-chirurgove@bulovka.cz");
        var externalMencl = GetExternalEmployee(employeeInfos, "externista-mencl@bulovka.cz");
        var externalCerny = GetExternalEmployee(employeeInfos, "externista-cerny@bulovka.cz");
        var externalKral = GetExternalEmployee(employeeInfos, "adam.kral@bulovka.cz");

        var externalEmployeeShiftAssignments = ArraySegment<ShiftAssignment>.Empty
            .Concat(GenerateExternalEmployeeShiftAssignments(5, externalChirurgove, new[] {2, 9, 16, 23, 30}))
            .Concat(GenerateExternalEmployeeShiftAssignments(5, externalMencl, new[] {4, 11}))
            .Concat(GenerateExternalEmployeeShiftAssignments(5, externalCerny, new[] {8, 22}))
            .Concat(GenerateExternalEmployeeShiftAssignments(5, externalKral, new[] {3, 10, 17, 24, 31, 13}));

        var predefinedShifts = existingShifts
            .Select(ss => new ShiftAssignment(GetEmployeeFromInfo(employeeInfos.Single(ei => ei.Email == ss.User.Email)), ss.Day, ss.Workplace))
            .Concat(externalEmployeeShiftAssignments)
            .Where(sa => sa.Shift.Day.ScheduledMonth == month)
            .ToArray();

        return new MonthScheduleParameters(
            month,
            new[] {externalChirurgove, externalMencl, externalCerny, externalKral},
            activeEmployeeMonths,
            CompetenceGroup.CompetenceGroups,
            lastMonthShifts,
            predefinedShifts,
            Workplace.WeekendInterchangeabilityGroups);
    }

    private static IEnumerable<ShiftAssignment> GenerateExternalEmployeeShiftAssignments(int monthNumber, Employee external1, int[] shiftDates)
    {
        return shiftDates.Select(d => new ShiftAssignment(external1, new Day(2023, monthNumber, d), Workplace.Urg));
    }

    private static Employee GetExternalEmployee(IReadOnlyCollection<EmployeeInfo> employeeInfos, string email)
    {
        var externalUserInfo = employeeInfos.SingleOrDefault(ei => ei.Email == email) ?? throw new InvalidOperationException($"Cannot find info for external user {email}");
        var externalUser = new Employee(externalUserInfo.Name, externalUserInfo.CompetenceLevel, externalUserInfo.Capabilities, externalUserInfo.Workload);
        return externalUser;
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
                userId == "auth0|637f84e6841043dd7a9d44a0" // Touficova
                    ? month.Days
                        .Where(d => d.DayOfWeek == DayOfWeek.Friday || d.DayOfWeek == DayOfWeek.Sunday)
                        .Select(d => new Leave(d, d, false))
                    : Array.Empty<Leave>())
            .ToArray();
}
