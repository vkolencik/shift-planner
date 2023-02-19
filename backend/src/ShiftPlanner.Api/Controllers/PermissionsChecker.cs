using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

class PermissionsChecker : IPermissionsChecker
{
    private readonly IUserAccessor _userAccessor;
    private readonly IClock _clock;

    public PermissionsChecker(IUserAccessor userAccessor, IClock clock)
    {
        _userAccessor = userAccessor;
        _clock = clock;
    }

    public bool CanReadRequirementsOfUser(string userId)
    {
        return userId == _userAccessor.GetLoggedInUserId() || _userAccessor.HasClaim(Claims.ReadRequirementsOfOthers);
    }

    public bool CanModifyRequirement(Day day, string userId)
    {
        if (_userAccessor.HasClaim(Claims.ModifyRequirementsOfOthers))
        {
            return true;
        }

        if (userId != _userAccessor.GetLoggedInUserId())
        {
            return false;
        }

        if (day <= _clock.Today)
        {
            return false;
        }

        return true;
    }

    public bool CanApproveRequirements() =>  _userAccessor.HasClaim(Claims.ApproveRequirementsOfOthers);
}
