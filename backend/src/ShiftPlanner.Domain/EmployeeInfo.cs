namespace ShiftPlanner.Domain;

public record EmployeeInfo(
    string Name,
    string Email,
    CompetenceLevel CompetenceLevel,
    Capabilities Capabilities,
    Workload Workload,
    bool Disabled);
