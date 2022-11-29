using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Services;

public interface IRequirementsService
{
    Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month);

    Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement);

    Task<DeleteResult> DeleteRequirement(Tenant tenant, string userId, Day day);
    
    Task<IReadOnlyCollection<UserRequirementModel>> GetRequirementsOverviewForMonth(Tenant tenant, ScheduledMonth month);
    Task<ApproveResult> Approve(Tenant tenant, string userId, Day day);
}
