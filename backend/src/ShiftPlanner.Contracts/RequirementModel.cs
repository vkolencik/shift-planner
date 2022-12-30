using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Contracts;

public record RequirementModel(
    Tenant Tenant,
    string UserId,
    Day Date,
    string RequirementType,
    bool? Approved = null);
