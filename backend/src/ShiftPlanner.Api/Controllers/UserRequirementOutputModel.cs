namespace ShiftPlanner.Api.Controllers;

public record UserRequirementOutputModel(
    string UserId,
    string UserName,
    string Date,
    string RequirementType);
