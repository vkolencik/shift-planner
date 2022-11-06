using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Services;

internal class InMemoryRequirementsRepository : IRequirementsRepository
{
    private readonly Dictionary<(Tenant, string, Day), string> _requirements;

    public InMemoryRequirementsRepository()
    {
        _requirements = new Dictionary<(Tenant, string, Day), string>();
    }

    public Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        var requirements = _requirements
            .Where(kvp => kvp.Key.Item1 == tenant)
            .Where(kvp => kvp.Key.Item2 == userId)
            .Where(kvp => month.Days.Contains(kvp.Key.Item3))
            .ToDictionary(
                kvp => kvp.Key.Item3,
                kvp => kvp.Value);
        return Task.FromResult(requirements);
    }

    public Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement)
    {
        _requirements[(tenant, userId, day)] = requirement;
        return Task.CompletedTask;
    }
}
