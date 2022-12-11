using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet.Tests.Integration;

public class GoogleSheetEmployeeServiceShould
{
    [Fact(Skip = "Will be enabled after implementing secret storage for Google credentials")]
    // [Fact]
    public async Task FetchEmployees()
    {
        var service = new GoogleSheetsEmployeeInfoService();

        var employees = await service.GetEmployees(Workplace.Workplaces);

        Assert.True(employees.Select(e => !e.Disabled).Count() > 20);
        
        var firstEmployee = employees.First();
        Assert.Equal("Tester Testovič", firstEmployee.Name);
        Assert.Equal("e2e-test-user-1@krtek.me", firstEmployee.Email);
        Assert.Equal(CompetenceLevel.Level2, firstEmployee.CompetenceLevel);
        Assert.Equal(
            new WorkplaceCapability[]
            {
                new WorkplaceCapability.Red(Workplace.A1),
                new WorkplaceCapability.Red(Workplace.A2),
                new WorkplaceCapability.Red(Workplace.A3),
                new WorkplaceCapability.Green(Workplace.Res, 30),
                new WorkplaceCapability.Green(Workplace.Kpr, 30),
                new WorkplaceCapability.Green(Workplace.Urg, 40)
            }, firstEmployee.Capabilities.WorkplaceCapabilities);
        Assert.Equal(Workload.Full, firstEmployee.Workload);
        Assert.True(firstEmployee.Disabled);

        var lastEmployee = employees.Last();
        Assert.Equal("Tester Testovič 2", lastEmployee.Name);
        Assert.Equal("e2e-test-user-2@krtek.me", lastEmployee.Email);
        Assert.Equal(CompetenceLevel.Level4, lastEmployee.CompetenceLevel);
        Assert.Equal(
            new WorkplaceCapability[]
            {
                new WorkplaceCapability.Green(Workplace.A1, 20),
                new WorkplaceCapability.Green(Workplace.A2, 30),
                new WorkplaceCapability.Green(Workplace.A3, 50),
                new WorkplaceCapability.Red(Workplace.Res),
                new WorkplaceCapability.Red(Workplace.Kpr),
                new WorkplaceCapability.Red(Workplace.Urg),
            }, lastEmployee.Capabilities.WorkplaceCapabilities);
        Assert.Equal(Workload.Workload07, lastEmployee.Workload);
        Assert.True(lastEmployee.Disabled);
    }
}
