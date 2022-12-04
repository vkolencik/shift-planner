using Moq;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.GoogleSheet;

namespace ShiftPlanner.ScheduleCalculator.Tests;

public class SchedulingParametersAggregatorShould
{
    private Mock<IEmployeeInfoService> _employeeInfoServiceMock;

    public SchedulingParametersAggregatorShould()
    {
        _employeeInfoServiceMock = new Mock<IEmployeeInfoService>();
    }

    [Fact]
    public async Task AggregateSchedulingParameters()
    {
        var month = new ScheduledMonth(2022, 12);

        var workplace = new Workplace("w1");
        var e1Capabilities = new Capabilities(new [] { new WorkplaceCapability.Green(workplace, 100), });
        var e2Capabilities = new Capabilities(new WorkplaceCapability[] { new WorkplaceCapability.Red(workplace), new WorkplaceCapability.Green(new Workplace("w2"), 100) });
        
        var employee1 = new Employee("e1", CompetenceLevel.Level1, e1Capabilities);
        var user1 = new User("uid1", "u1", "eml1@krtek.me");
        var employeeInfo1 = new EmployeeInfo("nm1", "eml1@krtek.me", employee1.CompetenceLevel, employee1.Capabilities, employee1.Workload, false);

        var employee2 = new Employee("e2", CompetenceLevel.Level2, e2Capabilities, Workload.Workload06);
        var user2 = new User("uid2", "u2", "eml2@krtek.me");
        var employeeInfo2 = new EmployeeInfo("nm2", "eml2@krtek.me", employee2.CompetenceLevel, employee2.Capabilities, employee2.Workload, false);

        var leave1 = new Leave(month.Days.First(), month.Days.Skip(1).First());
        var leave2 = new Leave(month.Days.Skip(10).First(), month.Days.Skip(11).First());
        IEnumerable<EmployeeMonth> expectedEmployeeMonths = new []
        {
            new EmployeeMonth(employee1, new [] {leave1, leave2}), 
            new EmployeeMonth(employee2, Array.Empty<Leave>()), 
        };

        _employeeInfoServiceMock
            .Setup(_ => _.GetEmployees(Workplace.Workplaces))
            .ReturnsAsync(new []
            {
                new EmployeeInfo(employee1.Name, user1.Email, employee1.CompetenceLevel, employee1.Capabilities, employee1.Workload, false),
                new EmployeeInfo(employee2.Name, user2.Email, employee2.CompetenceLevel, employee2.Capabilities, employee2.Workload, false),
            });
        
        var aggregator = new SchedulingParametersAggregator(_employeeInfoServiceMock.Object);

        var parameters = await aggregator.GetSchedulingParameters(month);

        Assert.Equal(month, parameters.Month);
        Assert.Equal(expectedEmployeeMonths, parameters.EmployeeMonths);
        Assert.Equal(CompetenceGroup.CompetenceGroups, parameters.CompetenceGroups);
    }
}
