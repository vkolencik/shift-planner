namespace ShiftPlanner.Domain;

public record Capabilities(IReadOnlyCollection<WorkplaceCapability> WorkplaceCapabilities)
{
    public ISet<Workplace> AllowedWorkplaces => new HashSet<Workplace>(
        WorkplaceCapabilities
            .Where(wc => wc is not WorkplaceCapability.Red)
            .Select(wc => wc.Workplace));
}

public abstract record WorkplaceCapability(Workplace Workplace)
{
    public record Red(Workplace Workplace) : WorkplaceCapability(Workplace)
    {
    }

    public record Yellow(Workplace Workplace) : WorkplaceCapability(Workplace)
    {
    }

    public record Green : WorkplaceCapability
    {
        public Green(Workplace workplace, uint preferencePercentage) : base(workplace)
        {
            if (preferencePercentage > 100)
            {
                throw new ArgumentException("Percentage cannot be over 100", nameof(preferencePercentage));
            }

            Workplace = workplace;
            this.PreferencePercentage = preferencePercentage;
        }

        public uint PreferencePercentage { get; }

        public void Deconstruct(out uint preferencePercentage)
        {
            preferencePercentage = PreferencePercentage;
        }
    }
}
