using System;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Moq;
using ShiftPlanner.Domain;
using Xunit;

namespace ShiftPlanner.Engine.Tests;

public class SchedulingEngineShould
{
    Mock<ISolver> _modelMock = new Mock<ISolver>();
    [Fact]
    public void CreateShiftSolverVariables()
    {
        var input = CreateBasicInput();
        CreateEngine(input);

        for (int day = 1; day <= 28; day++)
        {
            for (int employeeIndex = 1; employeeIndex <= input.Employees.Count; employeeIndex++)
            {
                for (int workplaceIndex = 1; workplaceIndex <= input.Workplaces.Count; workplaceIndex++)
                {
                    var (e, d, w) = (employeeIndex, day, workplaceIndex);
                    
                    _modelMock.Verify(_ => _.NewBoolVar(new VariableName($"shifts_e{e}_d{d}_w{w}")));
                }
            }
        }
    }

    [Fact]
    public void RunSolver()
    {
        var engine = CreateEngine(CreateEmptyInput());
        
        engine.Solve();
        
        _modelMock.Verify(_ => _.Solve());
    }

    [Fact]
    public void CreateOutputModel()
    {
        var input = CreateBasicInput();
        for (int day = 1; day <= 28; day++)
        {
            var d = day;
            
            // First employee is on the first workplace all the time:
            _modelMock.Setup(_ => _.Result(VariableName.Schedule(1, d, 1))).Returns(1);
            _modelMock.Setup(_ => _.Result(VariableName.Schedule(2, d, 1))).Returns(0);
            _modelMock.Setup(_ => _.Result(VariableName.Schedule(3, d, 1))).Returns(0);
            
            // Other two employees take turns at the second workplace:
            _modelMock.Setup(_ => _.Result(VariableName.Schedule(1, d, 2))).Returns(0);
            _modelMock.Setup(_ => _.Result(VariableName.Schedule(2, d, 2))).Returns(day % 2);
            _modelMock.Setup(_ => _.Result(VariableName.Schedule(3, d, 2))).Returns((day + 1) % 2);
            
        }
        var engine = CreateEngine(input);

        var schedule = engine.Solve();

        for (int d = 1; d <= 28; d++)
        {
            Assert.Equal(
                input.Employees.First(),
                schedule.GetAssignedEmployee(new DateOnly(2022, 2, d), input.Workplaces.First()));
        }

        for (int d = 1; d <= 28; d += 2)
        {
            Assert.Equal(
                input.Employees.ElementAt(1),
                schedule.GetAssignedEmployee(new DateOnly(2022, 2, d), input.Workplaces.ElementAt(1)));
        }

        for (int d = 2; d <= 28; d += 2)
        {
            Assert.Equal(
                input.Employees.ElementAt(2),
                schedule.GetAssignedEmployee(new DateOnly(2022, 2, d), input.Workplaces.ElementAt(1)));
        }
    }

    private static SchedulingParameters CreateEmptyInput()
    {
        return new SchedulingParameters(new ScheduledMonth(2022, 2), Array.Empty<Employee>(), Array.Empty<Workplace>());
    }

    private static SchedulingParameters CreateBasicInput()
    {
        return new SchedulingParameters(
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
    }

    private SchedulingEngine CreateEngine(SchedulingParameters input)
    {
        return new SchedulingEngine(_modelMock.Object, input);
    }
}
