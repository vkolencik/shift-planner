namespace ShiftPlanner.Domain;

public readonly record struct Tenant(string Name)
{
    public readonly bool Equals(Tenant other) => Name == other.Name;

    public readonly override int GetHashCode() => Name.GetHashCode();
}
