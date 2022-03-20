using Moq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Tests;

public class SchedulingEngineShould
{
    [Fact]
    public void CreateShiftVariables()
    {
        var modelMock = new Mock<IModel>();
        var solverMock = new Mock<ISolver>();
        var engine = new SchedulingEngine(modelMock.Object, solverMock.Object);

        var inputParams = new SchedulingParameters(
            new ScheduledMonth(2022, 2),
            new Employee[]
            {
                new Employee("Hynek"),
                new Employee("Vilem"),
                new Employee("Jarmila"),
            }, 
            new Workplace[]
            {
                new Workplace("ARO"),
                new Workplace("JIP"),
            });
        
        engine.CalculateSchedule(inputParams);

        for (int d = 1; d <= 28; d++)
        {
            for (int e = 1; e <= inputParams.Employees.Count; e++)
            {
                for (int w = 1; w <= inputParams.Workplaces.Count; w++)
                {
                    modelMock.Verify(_ => _.NewBoolVar(new VariableName($"shifts_e{e}_d{d}_w{w}")));
                }
            }
        }
    }
}
