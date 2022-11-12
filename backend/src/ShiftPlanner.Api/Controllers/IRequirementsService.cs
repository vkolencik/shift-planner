using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

public interface IRequirementsService
{
    Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month);
    Task SetRequirement(Tenant tenant, string userId, Day day, string requirement);
    Task DeleteRequirement(Tenant tenant, string userId, Day day);
}
