using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Contracts;

public record UserRequirementModel(string UserId, string UserName, Day Date, string RequirementType);
