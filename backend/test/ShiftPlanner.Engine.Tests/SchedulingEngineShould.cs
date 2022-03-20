using System;
using Moq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Tests;

public class SchedulingEngineShould
{
    Mock<ISolver> _modelMock = new Mock<ISolver>();
    [Fact]
    public void CreateShiftVariables()
    {
        var engine = CreateEngine();

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
                    _modelMock.Verify(_ => _.NewBoolVar(new VariableName($"shifts_e{e}_d{d}_w{w}")));
                }
            }
        }
    }

    private SchedulingEngine CreateEngine()
    {
        return new SchedulingEngine(_modelMock.Object);
    }

    [Fact]
    public void RunSolver()
    {
        var engine = CreateEngine();
        
        engine.CalculateSchedule(
            new SchedulingParameters(new ScheduledMonth(2022, 2), Array.Empty<Employee>(), Array.Empty<Workplace>()));
        
        _modelMock.Verify(_ => _.Solve());
    }
}
