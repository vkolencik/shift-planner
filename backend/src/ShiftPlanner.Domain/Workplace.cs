namespace ShiftPlanner.Domain;

public record Workplace(string Name, bool Optional = false)
{
    public static readonly Workplace A1 = new Workplace("A1", false);
    public static readonly Workplace A2 = new Workplace("A2", false);
    public static readonly Workplace A3 = new Workplace("A3", true);
    public static readonly Workplace Res = new Workplace("RES", false);
    public static readonly Workplace Kpr = new Workplace("KPR", true);
    public static readonly Workplace Urg = new Workplace("URG", true);

    public static readonly IList<Workplace> Workplaces = new[] {A1, A2, A3, Res, Kpr, Urg};

    public static readonly IReadOnlyCollection<ISet<Workplace>> WeekendInterchangeabilityGroups = Array.Empty<ISet<Workplace>>();
    // new [] {
        // new HashSet<Workplace> {Kpr, Urg}
    // };

    public override string ToString() => Name;
}
