using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Persistence;

public interface IRequirementsRepository
{
    Task<Dictionary<Day, string>> GetRequirements(
        Tenant tenant,
        string userId,
        ScheduledMonth month);

    Task SetRequirement(Requirement requirement);

    Task DeleteRequirement(
        Tenant tenant,
        string userId,
        Day day);

    Task<IReadOnlyCollection<Requirement>> GetRequirementsForMonth(Tenant tenant, ScheduledMonth month);
    
    Task<Requirement?> GetRequirement(Tenant tenant, string userId, Day day);
}
