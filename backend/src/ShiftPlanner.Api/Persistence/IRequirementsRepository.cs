using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

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
