using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

public interface IPermissionsChecker
{
    bool CanReadRequirementsOfUser(string userId);
    bool CanModifyRequirement(Day day, string userId, string requirementType);
    bool CanApproveRequirements();
}
