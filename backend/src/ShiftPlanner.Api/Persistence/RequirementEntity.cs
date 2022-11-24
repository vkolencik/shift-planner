using Amazon.DynamoDBv2.DataModel;

namespace ShiftPlanner.Api.Persistence;

[DynamoDBTable("Requirements")]
public class RequirementEntity
{
    [DynamoDBHashKey] public string? Key { get; set; }
    public string? TenantUserMonthKey { get; set; }
    public string? TenantMonthKey { get; set; }
    public string? Tenant { get; set; }
    public string? UserId { get; set; }
    public string? Date { get; set; }
    public string? Requirement { get; set; }
}
