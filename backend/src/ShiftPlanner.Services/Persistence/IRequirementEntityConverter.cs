using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Persistence;

public interface IRequirementEntityConverter
{
    RequirementEntity ToEntity(Requirement updatedRequirement);

    Dictionary<Day, string> RequirementEntitiesToMonthRequirements(IEnumerable<RequirementEntity> entities);
    
    string GetPartitionKey(Tenant tenant, string userId, Day day);

    string GetUserMonthKey(Tenant tenant, string userId, ScheduledMonth month);
    
    string GetMonthKey(Tenant tenant, ScheduledMonth month);
    Requirement EntityToModel(RequirementEntity entity);
}
