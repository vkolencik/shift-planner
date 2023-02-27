using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public interface IRequirementsService
{
    Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month);

    Task SetRequirement(Requirement requirement);

    Task DeleteRequirement(Tenant tenant, string userId, Day day);
    
    Task<IReadOnlyCollection<UserRequirementModel>> GetRequirementsOverviewForMonth(Tenant tenant, ScheduledMonth month);

    Task<Requirement?> GetRequirement(Tenant tenant, string userId, Day day);
    
    Task<ApproveResult> Approve(Tenant tenant, string userId, Day day);
}
