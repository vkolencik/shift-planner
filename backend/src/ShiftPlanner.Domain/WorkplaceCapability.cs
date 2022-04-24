namespace ShiftPlanner.Domain;

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
