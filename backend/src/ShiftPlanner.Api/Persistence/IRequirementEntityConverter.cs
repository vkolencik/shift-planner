using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

internal interface IRequirementEntityConverter
{
    RequirementEntity ToEntity(RequirementModel updatedRequirement);

    Dictionary<Day, string> RequirementEntitiesToMonthRequirements(IEnumerable<RequirementEntity> entities);
    
    string GetPartitionKey(Tenant tenant, string userId, Day day);

    string GetUserMonthKey(Tenant tenant, string userId, ScheduledMonth month);
    
    string GetMonthKey(Tenant tenant, ScheduledMonth month);
    RequirementModel EntityToModel(RequirementEntity entity);
}
