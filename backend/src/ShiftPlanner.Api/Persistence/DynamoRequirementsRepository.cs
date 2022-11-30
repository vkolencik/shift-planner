using System.Diagnostics;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;
using ShiftPlanner.Api.Contracts;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Persistence;

internal class DynamoRequirementsRepository : IRequirementsRepository
{
    private readonly DynamoOptions _options;
    private readonly IAmazonDynamoDB _dynamo;
    private readonly IRequirementEntityConverter _requirementEntityConverter;

    public DynamoRequirementsRepository(IAmazonDynamoDB dynamo, IRequirementEntityConverter requirementEntityConverter, IOptions<DynamoOptions> options)
    {
        _dynamo = dynamo;
        _requirementEntityConverter = requirementEntityConverter;
        _options = options.Value;
    }

    public async Task<Dictionary<Day, string>> GetRequirements(Tenant tenant, string userId, ScheduledMonth month)
    {
        var key = _requirementEntityConverter.GetUserMonthKey(tenant, userId, month);

        using var context = GetContext();

        var query = context.QueryAsync<RequirementEntity>(
            key,
            new DynamoDBOperationConfig {IndexName = "TenantUserMonthIndex"});

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var entities = await query.GetRemainingAsync();
        stopWatch.Stop();
        Console.Out.WriteLine($"Fetched {entities.Count} items from database in {stopWatch.ElapsedMilliseconds} ms");

        return _requirementEntityConverter.RequirementEntitiesToMonthRequirements(entities);
    }

    public async Task SetRequirement(RequirementModel updatedRequirement)
    {
        using var context = GetContext();

        var entity = _requirementEntityConverter.ToEntity(updatedRequirement);
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

    public async Task<IReadOnlyCollection<RequirementModel>> GetRequirementsForMonth(Tenant tenant, ScheduledMonth month)
    {
        var key = _requirementEntityConverter.GetMonthKey(tenant, month);

        using var context = GetContext();

        var query = context.QueryAsync<RequirementEntity>(
            key,
            new DynamoDBOperationConfig {IndexName = "TenantMonthIndex"});

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var entities = await query.GetRemainingAsync();
        stopWatch.Stop();
        
        Console.Out.WriteLine($"Fetched {entities.Count} items from database in {stopWatch.ElapsedMilliseconds} ms");

        return entities.Select(e => _requirementEntityConverter.EntityToModel(e)).ToArray();
    }

    public async Task<RequirementModel?> GetRequirement(Tenant tenant, string userId, Day day)
    {
        var key = _requirementEntityConverter.GetPartitionKey(tenant, userId, day);

        using var context = GetContext();

        var entity = await context.LoadAsync<RequirementEntity>(key);

        if (entity == null)
        {
            return null;
        }

        return _requirementEntityConverter.EntityToModel(entity);
    }
    
    private DynamoDBContext GetContext()
    {
        return new DynamoDBContext(_dynamo, new DynamoDBContextConfig
        {
            TableNamePrefix = _options.TableNamePrefix
        });
    }
}
