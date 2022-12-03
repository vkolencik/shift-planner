namespace ShiftPlanner.Domain;

public record Employee(
    string Name,
    string Email,
    CompetenceLevel CompetenceLevel,
    Capabilities Capabilities,
    Workload Workload,
    bool Disabled = false)
{
    public Employee(
        string name,
        CompetenceLevel competenceLevel,
        Capabilities capabilities)
        : this(name, "", competenceLevel, capabilities, Workload.Full)
    {
    }
}
