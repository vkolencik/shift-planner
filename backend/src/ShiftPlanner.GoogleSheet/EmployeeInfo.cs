using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet;

public record EmployeeInfo(
    string Name,
    string Email,
    CompetenceLevel CompetenceLevel,
    Capabilities Capabilities,
    Workload Workload,
    bool Disabled);
