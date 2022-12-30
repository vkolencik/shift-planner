namespace ShiftPlanner.Domain;

public record SolutionParameters(IReadOnlyDictionary<Employee, EmployeeStandard> RecordedStandards);
