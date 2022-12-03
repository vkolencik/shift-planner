using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet.Tests.Integration;

public class GoogleSheetEmployeeServiceShould
{
    [Fact]
    public async Task FetchEmployees()
    {
        var service = new GoogleSheetsEmployeeService();

        var employees = await service.GetEmployees(Workplace.Workplaces);

        Assert.True(employees.Select(e => !e.Disabled).Count() > 20);
        
        var firstEmployee = employees.First();
        Assert.Equal("Tester Testovič", firstEmployee.Name);
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
        Assert.Equal(Workload.Full, lastEmployee.Workload);
        Assert.True(lastEmployee.Disabled);
    }
}
