using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

internal class DynamoRequirementsRepository : IRequirementsRepository
{
    private IAmazonDynamoDB _dynamo;

    public DynamoRequirementsRepository(IAmazonDynamoDB dynamo)
    {
        _dynamo = dynamo;
    }

    public async Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        var monthStr = $"{month.Year}-{month.Month}";
        var key = $"v1:{tenant.Name}:{userId}:{month.Year}-{month.Month}";
        var context = new DynamoDBContext(_dynamo);
        var x = context.FromQueryAsync<RequirementEntity>(
            new QueryOperationConfig()
            {
                IndexName = "TenantUserMonthIndex",
                Limit = 31,
                Filter = new QueryFilter("TenantUserMonthKey", QueryOperator.Equal, new Primitive(key))
            });

        var y = context.QueryAsync<RequirementEntity>(
            key, new DynamoDBOperationConfig()
            {
                IndexName = "TenantUserMonthIndex"
            });

        var entities = await y.GetRemainingAsync();
        return entities.ToDictionary(e => DateToDay(e.Date), e => e.Requirement);
    }

    private static Day DateToDay(string date) => new Day(DateOnly.ParseExact(date, "yyyy-MM-dd"));

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
                Key = $"v1:{tenant.Name}:{userId}:{date}",
                TenantUserMonthKey = $"v1:{tenant.Name}:{userId}:{month}",
                TenantMonthKey = $"v1:{tenant.Name}:{month}",
                Tenant = tenant.Name,
                UserId = userId,
                Date =  date,
                Requirement = requirement
            });
    }

    public Task<DeleteResult> DeleteRequirement(Tenant tenant, string userId, Day day)
    {
        throw new NotImplementedException();
    }
}
