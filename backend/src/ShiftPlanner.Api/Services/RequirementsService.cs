using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Services;

public class RequirementsService : IRequirementsService
{
    private readonly IRequirementsRepository _repository;

    public RequirementsService(IRequirementsRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        return _repository.GetRequirements(tenant, userId, month);
    }

    public async Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement)
    {
        await _repository.SetRequirement(tenant, userId, day, requirement);
    }
}

public interface IRequirementsRepository
{
    Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month);

    Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement);
}

internal class InMemoryRequirementsRepository : IRequirementsRepository
{
    private readonly Dictionary<Day, string> _requirements;

    public InMemoryRequirementsRepository()
    {
        _requirements = new Dictionary<Day, string>();
    }

    public Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        return Task.FromResult(_requirements);
    }

    public Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement)
    {
        _requirements[day] = requirement;
        return Task.CompletedTask;
    }
}
