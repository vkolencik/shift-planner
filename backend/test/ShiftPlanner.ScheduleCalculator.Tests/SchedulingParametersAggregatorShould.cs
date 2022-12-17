using Moq;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.Services;
using ShiftPlanner.TestHelpers;

namespace ShiftPlanner.ScheduleCalculator.Tests;

public class SchedulingParametersAggregatorShould
{
    private Mock<IEmployeeInfoService> _employeeInfoServiceMock;
    private Mock<IRequirementsService> _requirementsServiceMock;
    private Mock<IUserService> _usersServiceMock;
    private Mock<IStoredScheduleService> _storedScheduleServiceMock;

    public SchedulingParametersAggregatorShould()
    {
        _employeeInfoServiceMock = new Mock<IEmployeeInfoService>();
        _requirementsServiceMock = new Mock<IRequirementsService>();
        _usersServiceMock = new Mock<IUserService>();
        _storedScheduleServiceMock = new Mock<IStoredScheduleService>();
    }

    [Fact]
    public async Task AggregateSchedulingParameters()
    {
        var tenant = TestData.Tenant;
        var month = new ScheduledMonth(2022, 12);
        var lastMonth = new ScheduledMonth(2022, 11);

        var workplace = new Workplace("w1");
        var e1Capabilities = new Capabilities(new[] {new WorkplaceCapability.Green(workplace, 100),});
        var e2Capabilities = new Capabilities(new WorkplaceCapability[] {new WorkplaceCapability.Red(workplace), new WorkplaceCapability.Green(new Workplace("w2"), 100)});

        var employee1 = new Employee("e1", CompetenceLevel.Level1, e1Capabilities);
        var user1 = new User("uid1", "u1", "eml1@krtek.me");

        var employee2 = new Employee("e2", CompetenceLevel.Level2, e2Capabilities, Workload.Workload06);
        var user2 = new User("uid2", "u2", "eml2@krtek.me");

        var employee3 = new Employee("e3", CompetenceLevel.Level3, e2Capabilities, Workload.Workload06);
        var user3 = new User("uid3", "u3", "eml3@krtek.me");

        var employee4 = new Employee("e4", CompetenceLevel.Level4, e2Capabilities, Workload.Workload06);
        var user4 = new User("uid4", "u4", "eml4@krtek.me");

        var leave1 = new Leave(month.Days.First(), month.Days.First());
        var leave2 = new Leave(month.Days.Skip(10).First(), month.Days.Skip(10).First(), false);
        var leave3 = new Leave(month.Days.Skip(20).First(), month.Days.Skip(20).First(), false);

        var lastMonthShifts = new[] {new ScheduledShift(new Day(lastMonth, 30), workplace, user1)};
        var expectedLastMonthShifts = new[] {new ShiftAssignment(employee1, new Day(lastMonth, 30), workplace)};

        var fixedShifts = new[] {new ScheduledShift(new Day(month, 1), workplace, user1)};
        var expectedFixedShifts = new[] {new ShiftAssignment(employee1, new Day(month, 1), workplace)};
        
        var lastMonthSchedule = new StoredSchedule(lastMonthShifts.Concat(fixedShifts).ToArray());

        _employeeInfoServiceMock
            .Setup(_ => _.GetEmployees(Workplace.Workplaces))
            .ReturnsAsync(
                new[]
                {
                    new EmployeeInfo(employee1.Name, user1.Email, employee1.CompetenceLevel, employee1.Capabilities, employee1.Workload, EmployeeStatus.Active),
                    new EmployeeInfo(employee2.Name, user2.Email, employee2.CompetenceLevel, employee2.Capabilities, employee2.Workload, EmployeeStatus.Active),
                    new EmployeeInfo(employee3.Name, user3.Email, employee3.CompetenceLevel, employee3.Capabilities, employee3.Workload, EmployeeStatus.Inactive),
                    new EmployeeInfo(employee4.Name, user4.Email, employee4.CompetenceLevel, employee4.Capabilities, employee4.Workload, EmployeeStatus.Disabled),
                });

        _requirementsServiceMock
            .Setup(_ => _.GetRequirementsOverviewForMonth(tenant, month))
            .ReturnsAsync(
                new UserRequirementModel[]
                {
                    // Counted as "not available":
                    new UserRequirementModel(user1.UserId, "", leave1.From, "leave", true),
                    new UserRequirementModel(user1.UserId, "", leave2.From, "hard-no", true),
                    new UserRequirementModel(user1.UserId, "", leave3.From, "only-morning", true),

                    // Counted as "available":
                    new UserRequirementModel(user1.UserId, "", month.Days.Skip(7).First(), "only-afternoon", true),
                    new UserRequirementModel(user1.UserId, "", month.Days.Skip(8).First(), "soft-yes", true),
                    new UserRequirementModel(user2.UserId, "", month.Days.Skip(17).First(), "only-afternoon", true),
                    new UserRequirementModel(user2.UserId, "", month.Days.Skip(18).First(), "soft-yes", true),
                });

        _usersServiceMock
            .Setup(_ => _.GetAllUsers())
            .ReturnsAsync(new[]
            {
                new User(user1.UserId, user1.UserName, user1.Email), 
                new User(user2.UserId, user2.UserName, user2.Email),
                new User(user3.UserId, user3.UserName, user3.Email),
                new User(user4.UserId, user4.UserName, user4.Email),
            });

        _storedScheduleServiceMock.Setup(_ => _.GetSchedule(lastMonth)).ReturnsAsync(lastMonthSchedule);

        IEnumerable<EmployeeMonth> expectedEmployeeMonths = new[]
        {
            new EmployeeMonth(employee1, new[] {leave1, leave2, leave3}),
            new EmployeeMonth(employee2, Array.Empty<Leave>()),
        };

        var aggregator = new SchedulingParametersAggregator(_employeeInfoServiceMock.Object, _requirementsServiceMock.Object, _usersServiceMock.Object, _storedScheduleServiceMock.Object);

        var parameters = await aggregator.GetSchedulingParameters(tenant, month);

        Assert.Equal(month, parameters.Month);
        Assert.Equal(2, parameters.EmployeeMonths.Count);
        Assert.Equal(expectedEmployeeMonths.First().Employee, parameters.EmployeeMonths.First().Employee);
        Assert.Equal(expectedEmployeeMonths.First().Leaves, parameters.EmployeeMonths.First().Leaves);
        Assert.Equal(expectedEmployeeMonths.Skip(1).First().Employee, parameters.EmployeeMonths.Skip(1).First().Employee);
        Assert.Equal(expectedEmployeeMonths.Skip(1).First().Leaves, parameters.EmployeeMonths.Skip(1).First().Leaves);
        Assert.Equal(CompetenceGroup.CompetenceGroups, parameters.CompetenceGroups);
        Assert.Equal(new HashSet<ShiftAssignment>(expectedLastMonthShifts), new HashSet<ShiftAssignment>(parameters.LastMonthShifts));
        Assert.Equal(new HashSet<ShiftAssignment>(expectedFixedShifts), new HashSet<ShiftAssignment>(parameters.FixedShifts));
        Assert.Equal(Workplace.WeekendInterchangeabilityGroups, parameters.WeekendInterchangeabilityGroups);
    }
}
