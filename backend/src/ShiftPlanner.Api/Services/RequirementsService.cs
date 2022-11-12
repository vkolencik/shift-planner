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

    public Task<DeleteResult> DeleteRequirement(Tenant tenant, string userId, Day day)
    {
        return _repository.DeleteRequirement(tenant, userId, day);
    }
}
