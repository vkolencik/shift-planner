using ShiftPlanner.Api.Controllers;

namespace ShiftPlanner.Api;

public class RequirementsService : IRequirementsService
{
    private readonly Dictionary<string,string> _requirements;

    public RequirementsService()
    {
        _requirements = new Dictionary<string, string>();
    }

    public Task<Dictionary<string, string>> GetRequirements(string userId, int year, int month)
    {
        return Task.FromResult(_requirements);
    }
}
