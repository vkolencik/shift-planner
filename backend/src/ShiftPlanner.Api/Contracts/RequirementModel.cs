using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Contracts;

public record RequirementModel(string UserId, Day Date, string RequirementType);
