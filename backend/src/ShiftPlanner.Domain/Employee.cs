namespace ShiftPlanner.Domain;

public record Employee(
    string Name,
    CompetenceLevel CompetenceLevel,
    Capabilities Capabilities,
    Workload Workload)
{
    public virtual bool Equals(Employee? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
