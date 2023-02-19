namespace ShiftPlanner.Api.Controllers;

public interface IUserAccessor
{
    string GetLoggedInUserId();
    bool HasClaim(string claim);
}
