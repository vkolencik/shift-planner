using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Persistence;

public interface IRequirementsRepository
{
    Task<Dictionary<Day, string>> GetRequirements(
        Tenant tenant,
        string userId,
        ScheduledMonth month);

    Task SetRequirement(RequirementModel requirement);

    Task<DeleteResult> DeleteRequirement(
        Tenant tenant,
        string userId,
        Day day);

    Task<IReadOnlyCollection<RequirementModel>> GetRequirementsForMonth(Tenant tenant, ScheduledMonth month);
    
    Task<RequirementModel?> GetRequirement(Tenant tenant, string userId, Day day);
}
