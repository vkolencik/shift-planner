using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services;

public interface IRequirementsService
{
    Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month);

    Task SetRequirement(Requirement requirement);

    Task<DeleteResult> DeleteRequirement(Tenant tenant, string userId, Day day);
    
    Task<IReadOnlyCollection<UserRequirementModel>> GetRequirementsOverviewForMonth(Tenant tenant, ScheduledMonth month);
    
    Task<ApproveResult> Approve(Tenant tenant, string userId, Day day);
}
