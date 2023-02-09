using System.Security.Claims;

namespace ShiftPlanner.Api.Controllers;

internal interface IPermissionsChecker
{
    bool CanReadRequirementsOfOthers();
    bool CanReadRequirementsOfUser(string userId);
}

class PermissionsChecker : IPermissionsChecker
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionsChecker(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool CanReadRequirementsOfUser(string userId)
    {
        // var loggedInUserId = GetUser().Identity.Name;
        //
        // return loggedInUserId != userId && !GetUser().Claims.Any(c => c.Type == "permissions" && c.Value == Claims.ReadRequirementsOfOthers)
    }

    private ClaimsPrincipal GetUser()
    {
        return _httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("User in context is null");
    }
}
