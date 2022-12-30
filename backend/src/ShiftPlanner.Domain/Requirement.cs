namespace ShiftPlanner.Domain;

public record Requirement(
    Tenant Tenant,
    string UserId,
    Day Date,
    string RequirementType,
    bool? Approved = null);
