namespace ShiftPlanner.Domain;

public record Capabilities
{
    public Capabilities(IReadOnlyCollection<WorkplaceCapability> workplaceCapabilities)
    {
        if (workplaceCapabilities == null) throw new ArgumentNullException(nameof(workplaceCapabilities));
        if (workplaceCapabilities.OfType<WorkplaceCapability.Green>().Sum(_ => _.PreferencePercentage) != 100)
        {
            throw new ArgumentException("Percentual capabilities must add up to 100 %", nameof(workplaceCapabilities));
        }
        
        this.WorkplaceCapabilities = workplaceCapabilities;
    }

    public ISet<Workplace> AllowedWorkplaces => new HashSet<Workplace>(
        WorkplaceCapabilities
            .Where(wc => wc is not WorkplaceCapability.Red)
            .Select(wc => wc.Workplace));

    public IReadOnlyCollection<WorkplaceCapability> WorkplaceCapabilities { get; init; }

    public void Deconstruct(out IReadOnlyCollection<WorkplaceCapability> WorkplaceCapabilities)
    {
        WorkplaceCapabilities = this.WorkplaceCapabilities;
    }
}
