using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

internal class DynamoRequirementsRepository : IRequirementsRepository
{
    private readonly IAmazonDynamoDB _dynamo;

    public DynamoRequirementsRepository(IAmazonDynamoDB dynamo)
    {
        _dynamo = dynamo;
    }

    public async Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        var key = $"v1:{tenant.Name}:{userId}:{month.Year}-{month.Month}";
        var context = GetContext();

        var query = context.QueryAsync<RequirementEntity>(
            key, new DynamoDBOperationConfig()
            {
                IndexName = "TenantUserMonthIndex"
            });

        var entities = await query.GetRemainingAsync();
        
        return entities.ToDictionary(e => DateToDay(e.Date), e => StringToRequirement(e.Requirement));
    }

    private string StringToRequirement(string? requirement) => requirement ?? throw new ArgumentNullException(nameof(requirement));

    private static Day DateToDay(string? date) => date is not null ? new Day(DateOnly.ParseExact(date, "yyyy-MM-dd")) : throw new ArgumentNullException(nameof(date));

    public async Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement)
    {
        var context = new DynamoDBContext(_dynamo);
        var date = day.Date.ToString("yyyy-MM-dd");
        var month = day.Date.ToString("yyyy-MM");

        await context.SaveAsync(
            new RequirementEntity
            {
                Key = GetKey(tenant, userId, day),
                TenantUserMonthKey = $"v1:{tenant.Name}:{userId}:{month}",
                TenantMonthKey = $"v1:{tenant.Name}:{month}",
                Tenant = tenant.Name,
                UserId = userId,
                Date =  date,
                Requirement = requirement
            });
    }

    public async Task<DeleteResult> DeleteRequirement(Tenant tenant, string userId, Day day)
    {
        var context = GetContext();
        var key = GetKey(tenant, userId, day);
        var existingRequirement = await context.LoadAsync<RequirementEntity>(key);
        if (existingRequirement is null)
        {
            return DeleteResult.NotFound;
        }
        
        await context.DeleteAsync<RequirementEntity>(key);
        return DeleteResult.Deleted;
    }

    private static string GetKey(Tenant tenant, string userId, Day day)
    {
        var date = day.Date.ToString("yyyy-MM-dd");
        return $"v1:{tenant.Name}:{userId}:{date}";
    }

    private DynamoDBContext GetContext()
    {
        return new DynamoDBContext(_dynamo);
    }
}
