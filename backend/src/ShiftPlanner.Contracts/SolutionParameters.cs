using ShiftPlanner.Domain;

namespace ShiftPlanner.Contracts;

public record SolutionParameters(IReadOnlyDictionary<Employee, EmployeeStandard> RecordedStandards);
