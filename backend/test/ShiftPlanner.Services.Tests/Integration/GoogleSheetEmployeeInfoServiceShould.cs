using Amazon;
using Amazon.SecretsManager;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Infra;
using ShiftPlanner.Services.Services.Google;

namespace ShiftPlanner.Services.Tests.Integration;

public class GoogleSheetEmployeeInfoServiceShould
{
    [Fact(Skip = "Skipped until the new format is implemented")]
    public async Task FetchEmployees()
    {
        var service = new GoogleSheetsEmployeeInfoService(new AwsSecretsManagerClient(new AmazonSecretsManagerClient(RegionEndpoint.EUCentral1)));

        var employees = await service.GetEmployees(Workplace.Workplaces);

        Assert.True(employees.Select(e => e.EmployeeStatus == EmployeeStatus.Active).Count() > 20);

        var testerEmployee = employees.First();
        Assert.Equal("Tester Testovič", testerEmployee.Name);
        Assert.Equal("e2e-test-user-1@krtek.me", testerEmployee.Email);
        Assert.Equal(CompetenceLevel.Level2, testerEmployee.CompetenceLevel);
        Assert.Equal(
            new WorkplaceCapability[]
            {
                new WorkplaceCapability.Red(Workplace.A1),
                new WorkplaceCapability.Red(Workplace.A2),
                new WorkplaceCapability.Red(Workplace.A3),
                new WorkplaceCapability.Green(Workplace.Res, 30),
                new WorkplaceCapability.Green(Workplace.Kpr, 30),
                new WorkplaceCapability.Green(Workplace.Urg, 40)
            }, testerEmployee.Capabilities.WorkplaceCapabilities);
        Assert.Equal(new Workload(27, 42), testerEmployee.Workload);
        Assert.Equal(EmployeeStatus.Disabled, testerEmployee.EmployeeStatus);

        var tester2Employee = employees.Last();
        Assert.Equal("Tester Testovič 2", tester2Employee.Name);
        Assert.Equal("e2e-test-user-2@krtek.me", tester2Employee.Email);
        Assert.Equal(CompetenceLevel.Level4, tester2Employee.CompetenceLevel);
        Assert.Equal(
            new WorkplaceCapability[]
            {
                new WorkplaceCapability.Green(Workplace.A1, 20),
                new WorkplaceCapability.Green(Workplace.A2, 30),
                new WorkplaceCapability.Green(Workplace.A3, 50),
                new WorkplaceCapability.Red(Workplace.Res),
                new WorkplaceCapability.Red(Workplace.Kpr),
                new WorkplaceCapability.Red(Workplace.Urg),
            }, tester2Employee.Capabilities.WorkplaceCapabilities);
        Assert.Equal(new Workload(0, 1), tester2Employee.Workload);
        Assert.Equal(EmployeeStatus.Disabled, testerEmployee.EmployeeStatus);
    }
}
