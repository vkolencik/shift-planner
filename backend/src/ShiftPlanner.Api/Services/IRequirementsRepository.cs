using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Services;

public interface IRequirementsRepository
{
    Task<Dictionary<Day, string>> GetRequirements(
        Tenant tenant,
        string userId,
        ScheduledMonth month);

    Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement);

    Task<DeleteResult> DeleteRequirement(
        Tenant tenant,
        string userId,
        Day day);
}
