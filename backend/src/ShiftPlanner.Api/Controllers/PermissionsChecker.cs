namespace ShiftPlanner.Api.Controllers;

class PermissionsChecker : IPermissionsChecker
{
    private readonly IUserAccessor _userAccessor;

    public PermissionsChecker(IUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;
    }

    public bool CanReadRequirementsOfOthers()
    {
        return false;
    }

    public bool CanReadRequirementsOfUser(string userId)
    {
        return userId == _userAccessor.GetLoggedInUserId() || _userAccessor.HasClaim(Claims.ReadRequirementsOfOthers);
    }

    public bool CanModifyRequirementsOfUser(string userId)
    {
        return userId == _userAccessor.GetLoggedInUserId() || _userAccessor.HasClaim(Claims.ModifyRequirementsOfOthers);
    }

    public bool CanApproveRequirements() =>  _userAccessor.HasClaim(Claims.ApproveRequirementsOfOthers);
}
