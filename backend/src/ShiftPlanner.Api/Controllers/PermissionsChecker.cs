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

        return IsModifiableOnDay(day);
    }

    private bool IsModifiableOnDay(Day day)
    {
        var today = _clock.Today;

        var activeMonthAfterMonths = today.DayOfMonth < 25 ? 1 : 2;
        var activeMonth = today.Date.AddMonths(activeMonthAfterMonths);
        
        if (day.Year != activeMonth.Year || day.Month != activeMonth.Month)
        {
            return false;
        }

        var isReadOnly = activeMonthAfterMonths == 1 && today.DayOfMonth >= 5;
        
        return !isReadOnly;
    }

    public bool CanApproveRequirements() =>  _userAccessor.HasClaim(Claims.ApproveRequirementsOfOthers);
}
