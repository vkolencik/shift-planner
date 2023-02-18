using System.Security.Claims;

namespace ShiftPlanner.Api.Controllers;

class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    private ClaimsPrincipal GetUser()
    {
        return _httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("User in context is null");
    }

    public string GetLoggedInUserId() => GetUser().Identity.Name;
    public bool HasClaim(string claim) => GetUser().Claims.Any(c => c.Type == "permissions" && c.Value == claim);
}
