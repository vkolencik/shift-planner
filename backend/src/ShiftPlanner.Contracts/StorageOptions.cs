namespace ShiftPlanner.Contracts;

public class StorageOptions
{
    public StorageOptions()
    {
        StoragePrefix = "";
    }

    public const string SectionName = "StorageOptions";

    public string StoragePrefix { get; set; }
}
