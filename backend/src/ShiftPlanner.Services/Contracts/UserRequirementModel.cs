using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Contracts;

public record UserRequirementModel(string UserId, string UserName, Day Date, string RequirementType, bool? Approved);
