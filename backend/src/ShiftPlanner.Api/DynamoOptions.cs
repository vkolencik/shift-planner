namespace ShiftPlanner.Api;

public class DynamoOptions
{
    public const string SectionName = "DynamoOptions";

    public string? TableNamePrefix { get; set; }
}
