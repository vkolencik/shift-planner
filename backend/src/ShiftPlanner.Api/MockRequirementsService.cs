using ShiftPlanner.Api.Controllers;

namespace ShiftPlanner.Api;

public class MockRequirementsService : IRequirementsService
{
    private Dictionary<string,string> _requirements;

    public MockRequirementsService()
    {
        _requirements = new Dictionary<string, string>
        {
            ["2022-12-06"] = "soft-yes",
            ["2022-12-08"] = "soft-yes",
            ["2022-12-21"] = "leave",
            ["2022-12-13"] = "hard-no"    
        };
    }

    public Task<Dictionary<string, string>> GetRequirements(string userId, int year, int month)
    {
        return Task.FromResult(_requirements);
    }
}
