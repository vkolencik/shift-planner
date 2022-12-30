using ShiftPlanner.Contracts;
using ShiftPlanner.Contracts.Services;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Services;

internal class RequirementsService : IRequirementsService
{
    private readonly IRequirementsRepository _repository;
    private readonly IUserService _userService;

    public RequirementsService(IRequirementsRepository repository, IUserService userService)
    {
        _repository = repository;
        _userService = userService;
    }

    public Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        return _repository.GetRequirements(tenant, userId, month);
    }

    public async Task SetRequirement(RequirementModel requirement)
    {
        await _repository.SetRequirement(requirement);
    }

    public Task<DeleteResult> DeleteRequirement(Tenant tenant, string userId, Day day)
    {
        return _repository.DeleteRequirement(tenant, userId, day);
    }

    public async Task<IReadOnlyCollection<UserRequirementModel>> GetRequirementsOverviewForMonth(Tenant tenant, ScheduledMonth month)
    {
        var requirements = await _repository.GetRequirementsForMonth(tenant, month);
        var users = await _userService.GetAllUsers();
        // TODO: handle deleted user case here
        var userRequirements = requirements.Select(r => new UserRequirementModel(r.UserId, users.Single(u => u.UserId == r.UserId).UserName, r.Date, r.RequirementType, r.Approved)).ToArray();
        return userRequirements;
    }

    public async Task<ApproveResult> Approve(Tenant tenant, string userId, Day day)
    {
        var existingRequirement = await _repository.GetRequirement(tenant, userId, day);

        if (existingRequirement is null)
        {
            return ApproveResult.NotFound;
        }

        if (existingRequirement.RequirementType != "leave")
        {
            return ApproveResult.Failure;
        }
        
        await _repository.SetRequirement(new RequirementModel(tenant, userId, day, "leave", true));
        
        return ApproveResult.Success;
    }
}
