namespace ShiftPlanner.Domain;

public record Capabilities(IReadOnlyCollection<WorkplaceCapability> WorkplaceCapabilities)
{
    public ISet<Workplace> AllowedWorkplaces => new HashSet<Workplace>(
        WorkplaceCapabilities
            .Where(wc => wc is not WorkplaceCapability.Red)
            .Select(wc => wc.Workplace));
}
