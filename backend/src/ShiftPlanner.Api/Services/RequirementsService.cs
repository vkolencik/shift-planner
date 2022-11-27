using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Api.Persistence;
using ShiftPlanner.Api.Services.Users;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Services;

public class RequirementsService : IRequirementsService
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

    public async Task<IReadOnlyCollection<UserRequirementModel>> GetRequirementsOverviewForMonth()
    {
        var requirements = await _repository.GetRequirementsForMonth();
        var users = await _userService.GetAllUsers();
        var userRequirements = requirements.Select(r => new UserRequirementModel(r.UserId, users.Single(u => u.UserId == r.UserId).UserName, r.Date, r.RequirementType)).ToArray();
        return userRequirements;
    }
}
