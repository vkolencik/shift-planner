namespace ShiftPlanner.Api.Controllers;

public interface IPermissionsChecker
{
    bool CanReadRequirementsOfOthers();
    bool CanReadRequirementsOfUser(string userId);
    bool CanModifyRequirementsOfUser(string userId);
    bool CanApproveRequirements();
}
