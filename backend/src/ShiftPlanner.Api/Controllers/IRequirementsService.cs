using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Controllers;

public interface IRequirementsService
{
    Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month);
}
