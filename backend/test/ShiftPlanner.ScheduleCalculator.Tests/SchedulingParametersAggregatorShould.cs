using Moq;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Services;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Domain;
using ShiftPlanner.GoogleSheet;
using ShiftPlanner.TestHelpers;

namespace ShiftPlanner.ScheduleCalculator.Tests;

public class SchedulingParametersAggregatorShould
{
    private Mock<IEmployeeInfoService> _employeeInfoServiceMock;
    private Mock<IRequirementsService> _requirementsServiceMock;
    private Mock<IUserService> _usersServiceMock;

    public SchedulingParametersAggregatorShould()
    {
        _employeeInfoServiceMock = new Mock<IEmployeeInfoService>();
        _requirementsServiceMock = new Mock<IRequirementsService>();
        _usersServiceMock = new Mock<IUserService>();
    }

    [Fact]
    public async Task AggregateSchedulingParameters()
    {
        var tenant = TestData.Tenant;
        var month = new ScheduledMonth(2022, 12);

        var workplace = new Workplace("w1");
        var e1Capabilities = new Capabilities(new[] {new WorkplaceCapability.Green(workplace, 100),});
        var e2Capabilities = new Capabilities(new WorkplaceCapability[] {new WorkplaceCapability.Red(workplace), new WorkplaceCapability.Green(new Workplace("w2"), 100)});

        var employee1 = new Employee("e1", CompetenceLevel.Level1, e1Capabilities);
        var user1 = new User("uid1", "u1", "eml1@krtek.me");

        var employee2 = new Employee("e2", CompetenceLevel.Level2, e2Capabilities, Workload.Workload06);
        var user2 = new User("uid2", "u2", "eml2@krtek.me");

        var leave1 = new Leave(month.Days.First(), month.Days.First());
        var leave2 = new Leave(month.Days.Skip(10).First(), month.Days.Skip(10).First());
        var leave3 = new Leave(month.Days.Skip(20).First(), month.Days.Skip(20).First());

        _employeeInfoServiceMock
            .Setup(_ => _.GetEmployees(Workplace.Workplaces))
            .ReturnsAsync(
                new[]
                {
                    new EmployeeInfo(employee1.Name, user1.Email, employee1.CompetenceLevel, employee1.Capabilities, employee1.Workload, false),
                    new EmployeeInfo(employee2.Name, user2.Email, employee2.CompetenceLevel, employee2.Capabilities, employee2.Workload, false),
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
            .ReturnsAsync(new [] { new User(user1.UserId, user1.UserName, user1.Email), new User(user2.UserId, user2.UserName, user2.Email)});

        IEnumerable<EmployeeMonth> expectedEmployeeMonths = new[]
        {
            new EmployeeMonth(employee1, new[] {leave1, leave2, leave3}),
            new EmployeeMonth(employee2, Array.Empty<Leave>()),
        };

        var aggregator = new SchedulingParametersAggregator(_employeeInfoServiceMock.Object, _requirementsServiceMock.Object, _usersServiceMock.Object);

        var parameters = await aggregator.GetSchedulingParameters(tenant, month);

        Assert.Equal(month, parameters.Month);
        Assert.Equal(expectedEmployeeMonths.First().Employee, parameters.EmployeeMonths.First().Employee);
        Assert.Equal(expectedEmployeeMonths.First().Leaves, parameters.EmployeeMonths.First().Leaves);
        Assert.Equal(expectedEmployeeMonths.Skip(1).First().Employee, parameters.EmployeeMonths.Skip(1).First().Employee);
        Assert.Equal(expectedEmployeeMonths.Skip(1).First().Leaves, parameters.EmployeeMonths.Skip(1).First().Leaves);
        Assert.Equal(CompetenceGroup.CompetenceGroups, parameters.CompetenceGroups);
    }
}
