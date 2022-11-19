using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ShiftPlanner.Api.Controllers;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

internal class DynamoRequirementsRepository : IRequirementsRepository
{
    private readonly IAmazonDynamoDB _dynamo;
    private readonly IRequirementEntityConverter _requirementEntityConverter;

    public DynamoRequirementsRepository(IAmazonDynamoDB dynamo, IRequirementEntityConverter requirementEntityConverter)
    {
        _dynamo = dynamo;
        _requirementEntityConverter = requirementEntityConverter;
    }

    public async Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        var key = _requirementEntityConverter.GetUserMonthKey(tenant, userId, month);
        
        using var context = GetContext();

        var query = context.QueryAsync<RequirementEntity>(
            key,
            new DynamoDBOperationConfig {IndexName = "TenantUserMonthIndex"});

        var entities = await query.GetRemainingAsync();

        return _requirementEntityConverter.RequirementEntitiesToMonthRequirements(entities);
    }

    public async Task SetRequirement(
        Tenant tenant,
        string userId,
        Day day,
        string requirement)
    {
        using var context = GetContext();

        var entity = _requirementEntityConverter.ToEntity(tenant, userId, day, requirement);
        await context.SaveAsync(entity);
    }

    public async Task<DeleteResult> DeleteRequirement(Tenant tenant, string userId, Day day)
    {
        using var context = GetContext();
        var key = _requirementEntityConverter.GetPartitionKey(tenant, userId, day);
        var existingRequirement = await context.LoadAsync<RequirementEntity>(key);
        if (existingRequirement is null)
        {
            return DeleteResult.NotFound;
        }

        await context.DeleteAsync<RequirementEntity>(key);
        return DeleteResult.Deleted;
    }

    private DynamoDBContext GetContext()
    {
        return new DynamoDBContext(_dynamo);
    }
}
