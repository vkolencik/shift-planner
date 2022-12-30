using System.Collections.Generic;
using ShiftPlanner.Domain;

namespace ShiftPlanner.TestHelpers;

public record SolverParameters(
    IReadOnlyCollection<Day> Days,
    IReadOnlyCollection<Employee> Employees,
    IReadOnlyCollection<Workplace> Workplaces) : ISolverParameters;
