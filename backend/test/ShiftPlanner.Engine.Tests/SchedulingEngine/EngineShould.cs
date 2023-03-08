using System;
using System.Linq;
using Moq;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using ShiftPlanner.Rules;
using Xunit;

namespace ShiftPlanner.Engine.Tests.SchedulingEngine;

public class EngineShould
{
    Mock<ISolver> _solverMock = new Mock<ISolver>();

    [Fact]
    public void CreateShiftSolverVariables()
    {
        var input = CreateBasicInput();
        CreateEngine(input);

        for (int day = 1; day <= input.Days.Count; day++)
        {
            for (int employeeIndex = 1; employeeIndex <= input.Employees.Count; employeeIndex++)
            {
                for (int workplaceIndex = 1; workplaceIndex <= input.Workplaces.Count; workplaceIndex++)
                {
                    var (e, d, w) = (employeeIndex, day, workplaceIndex);

                    _solverMock.Verify(_ => _.NewBoolVar(new VariableName($"shifts_e{e}_d{d}_w{w}")));
                }
            }
        }
    }

    [Fact]
    public void RunSolver()
    {
        var engine = CreateEngine(CreateEmptyInput());

        engine.Solve();

        _solverMock.Verify(_ => _.Solve());
    }

    [Fact]
    public void CreateOutputModel()
    {
        var input = CreateBasicInput();
        for (int day = 1; day <= input.Days.Count; day++)
        {
            var d = day;

            // First employee is on the first workplace all the time:
            _solverMock.Setup(_ => _.Result(VariableName.Schedule(1, d, 1))).Returns(1);
            _solverMock.Setup(_ => _.Result(VariableName.Schedule(2, d, 1))).Returns(0);
            _solverMock.Setup(_ => _.Result(VariableName.Schedule(3, d, 1))).Returns(0);

            // Other two employees take turns at the second workplace:
            _solverMock.Setup(_ => _.Result(VariableName.Schedule(1, d, 2))).Returns(0);
            _solverMock.Setup(_ => _.Result(VariableName.Schedule(2, d, 2))).Returns(day % 2);
            _solverMock.Setup(_ => _.Result(VariableName.Schedule(3, d, 2))).Returns((day + 1) % 2);
        }

        var engine = CreateEngine(input);

        var result = engine.Solve();

        var schedule = Assert.IsType<PlannedSchedule>(result);

        for (int d = 1; d <= input.Days.Count; d++)
        {
            Assert.Equal(
                input.Employees.First(),
                schedule.GetAssignedEmployee(new Shift(new Day(2022, 2, d), input.Workplaces.First())));
        }

        for (int d = 1; d <= input.Days.Count; d += 2)
        {
            Assert.Equal(
                input.Employees.ElementAt(1),
                schedule.GetAssignedEmployee(new Shift(new Day(2022, 2, d), input.Workplaces.ElementAt(1))));
        }

        for (int d = 2; d <= input.Days.Count; d += 2)
        {
            Assert.Equal(
                input.Employees.ElementAt(2),
                schedule.GetAssignedEmployee(new Shift(new Day(2022, 2, d), input.Workplaces.ElementAt(1))));
        }
    }

    [Fact]
    public void CreateSumConstraint()
    {
        var input = CreateBasicInput();
        var engine = CreateEngine(input);

        var shifts = new[]
        {
            new ShiftAssignment(input.Employees.ElementAt(0), input.Days.ElementAt(0), input.Workplaces.ElementAt(0)),
            new ShiftAssignment(input.Employees.ElementAt(1), input.Days.ElementAt(1), input.Workplaces.ElementAt(1))
        };

        engine.AddSumConstraint(shifts, Constraint.Gte, 2);

        _solverMock.Verify(
            s => s.AddSumConstraint(new[] {VariableName.Schedule(1, 1, 1), VariableName.Schedule(2, 2, 2)}, Constraint.Gte, 2));
    }

    [Fact]
    public void CreateVariableConstraint()
    {
        var input = CreateBasicInput();
        var engine = CreateEngine(input);

        var shift = new ShiftAssignment(
            input.Employees.ElementAt(0), input.Days.ElementAt(0), input.Workplaces.ElementAt(0));

        engine.AddVariableConstraint(shift, Constraint.Lte, 7);

        _solverMock.Verify(
            s => s.AddVariableConstraint(VariableName.Schedule(1, 1, 1), Constraint.Lte, 7));
    }

    [Fact]
    public void CreateSumRelationConstraint()
    {
        var input = CreateBasicInput();
        var engine = CreateEngine(input);

        var shiftA1 = new ShiftAssignment(
            input.Employees.ElementAt(0), input.Days.ElementAt(0), input.Workplaces.ElementAt(0));
        var shiftA2 = new ShiftAssignment(
            input.Employees.ElementAt(0), input.Days.ElementAt(1), input.Workplaces.ElementAt(0));
        var shiftB1 = new ShiftAssignment(
            input.Employees.ElementAt(1), input.Days.ElementAt(0), input.Workplaces.ElementAt(0));
        var shiftB2 = new ShiftAssignment(
            input.Employees.ElementAt(1), input.Days.ElementAt(1), input.Workplaces.ElementAt(0));

        engine.AddSumRelationConstraint(new[] {shiftA1, shiftA2}, Constraint.Eq, new[] {shiftB1, shiftB2});

        _solverMock.Verify(
            s => s.AddSumRelationConstraint(new[] {VariableName.Schedule(1, 1, 1), VariableName.Schedule(1, 2, 1)}, Constraint.Eq, new[] {VariableName.Schedule(2, 1, 1), VariableName.Schedule(2, 2, 1)}));
    }

    [Fact]
    public void ReturnNull_WhenSolverDoesntFindSolution()
    {
        var engine = CreateEngine(CreateEmptyInput());

        _solverMock.Setup(_ => _.SolverStatus).Returns(SolverStatus.NotFound);

        var schedulingResult = engine.Solve();

        Assert.Null(schedulingResult);
    }

    private static ISolverParameters CreateEmptyInput()
    {
        return new SolverParameters(new ScheduledMonth(2022, 2).Days, Array.Empty<Employee>(), Array.Empty<Workplace>());
    }

    private static ISolverParameters CreateBasicInput()
    {
        var workplace1 = new Workplace("ARO");
        var workplaces = new Workplace[]
        {
            workplace1,
            new Workplace("JIP"),
        };

        return new SolverParameters(
            Enumerable.Range(1, 5).Select(d => new Day(2022, 2, d)).ToArray(),
            new Employee[]
            {
                new Employee("Hynek", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(workplace1, 100)}), new Workload(5, 7)),
                new Employee("Vilem", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(workplace1, 100)}), new Workload(5, 7)),
                new Employee("Jarmila", CompetenceLevel.Level1, new Capabilities(new[] {new WorkplaceCapability.Green(workplace1, 100)}), new Workload(5, 7)),
            },
            workplaces);
    }

    private Engine.SchedulingEngine.Engine CreateEngine(ISolverParameters input)
    {
        return new Engine.SchedulingEngine.Engine(_solverMock.Object, input);
    }
}
