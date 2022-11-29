using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

internal class RequirementEntityConverter : IRequirementEntityConverter
{
    public RequirementEntity ToEntity(
        Tenant tenant,
        string userId,
        Day day,
        string requirement)
    {
        var date = GetDateString(day);
        var month = GetMonthString(day);

        return new RequirementEntity
        {
            Key = GetPartitionKey(tenant, userId, day),
            TenantUserMonthKey = GetUserMonthKey(tenant, userId, month),
            TenantMonthKey = GetMonthKey(tenant, month),
            Tenant = tenant.Name,
            UserId = userId,
            Date = date,
            Requirement = requirement
        };
    }

    public Dictionary<Day, string> RequirementEntitiesToMonthRequirements(IEnumerable<RequirementEntity> entities)
    {
        return entities.ToDictionary(e => DateToDay(e.Date), e => StringToRequirement(e.Requirement));
    }

    public IReadOnlyCollection<RequirementModel> RequirementEntitiesToRequirements(IEnumerable<RequirementEntity> entities)
    {
        return entities
            .Select(EntityToModel)
            .ToArray();
    }

    public RequirementModel EntityToModel(RequirementEntity entity)
    {
        return new RequirementModel(
            UserId: StringToUserId(entity.UserId),
            Date: DateToDay(entity.Date),
            RequirementType: StringToRequirement(entity.Requirement));
    }

    private string StringToUserId(string? userId) => userId ?? throw new ArgumentNullException(nameof(userId));
    
    private string StringToRequirement(string? requirement) => requirement ?? throw new ArgumentNullException(nameof(requirement));

    private static Day DateToDay(string? date) => date is not null ? new Day(DateOnly.ParseExact(date, "yyyy-MM-dd")) : throw new ArgumentNullException(nameof(date));

    public string GetPartitionKey(Tenant tenant, string userId, Day day)
    {
        return GetPartitionKey(tenant, userId, GetDateString(day));
    }

    public string GetUserMonthKey(Tenant tenant, string userId, ScheduledMonth month)
    {
        return GetUserMonthKey(tenant, userId, GetMonthString(month));
    }

    public string GetMonthKey(Tenant tenant, ScheduledMonth month)
    {
        return GetMonthKey(tenant, GetMonthString(month));
    }

    private static string GetPartitionKey(Tenant tenant, string userId, string date) => $"v1:{tenant.Name}:{userId}:{date}";
    private static string GetUserMonthKey(Tenant tenant, string userId, string month) => $"v1:{tenant.Name}:{userId}:{month}";
    private static string GetMonthKey(Tenant tenant, string month) => $"v1:{tenant.Name}:{month}";

    private static string GetDateString(Day day) => day.Date.ToString("yyyy-MM-dd");
    private static string GetMonthString(ScheduledMonth month) => $"{month.Year}-{month.Month:D2}";
    private static string GetMonthString(Day day) => day.Date.ToString("yyyy-MM");
}
