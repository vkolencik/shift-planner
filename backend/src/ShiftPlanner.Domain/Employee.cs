namespace ShiftPlanner.Domain;

public record Employee(string Name,
    CompetenceLevel CompetenceLevel,
    Capabilities Capabilities,
    Workload Workload)
{
    public Employee(
        string name,
        CompetenceLevel competenceLevel,
        Capabilities capabilities)
        : this(name, competenceLevel, capabilities, Workload.Full)
    {
    }
}
