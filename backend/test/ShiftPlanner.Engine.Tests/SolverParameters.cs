using System;
using System.Collections.Generic;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine.Tests;

public record SolverParameters(
    IReadOnlyCollection<Day> Days,
    IReadOnlyCollection<Employee> Employees,
    IReadOnlyCollection<Workplace> Workplaces) : ISolverParameters
{
    public IReadOnlyCollection<Employee> AllEmployees => Array.Empty<Employee>();
}
