using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

internal interface IRequirementEntityConverter
{
    RequirementEntity ToEntity(
        Tenant tenant,
        string userId,
        Day day,
        string requirement);

    Dictionary<Day, string> RequirementEntitiesToMonthRequirements(IEnumerable<RequirementEntity> entities);
    
    IReadOnlyCollection<RequirementModel> RequirementEntitiesToRequirements(IEnumerable<RequirementEntity> entities);

    string GetPartitionKey(Tenant tenant, string userId, Day day);

    string GetUserMonthKey(Tenant tenant, string userId, ScheduledMonth month);
    
    string GetMonthKey(Tenant tenant, ScheduledMonth month);
}
