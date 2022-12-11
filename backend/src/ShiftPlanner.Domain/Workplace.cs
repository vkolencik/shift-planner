namespace ShiftPlanner.Domain;

public record Workplace(string Name)
{
    public static readonly Workplace A1 = new Workplace("A1");
    public static readonly Workplace A2 = new Workplace("A2");
    public static readonly Workplace A3 = new Workplace("A3");
    public static readonly Workplace Res = new Workplace("RES");
    public static readonly Workplace Kpr = new Workplace("KPR");
    public static readonly Workplace Urg = new Workplace("URG");

    public static IList<Workplace> Workplaces = new[] {A1, A2, A3, Res, Kpr, Urg};

    public static IReadOnlyCollection<ISet<Workplace>> WeekendInterchangeabilityGroups =
    new [] {
        new HashSet<Workplace> {Kpr, Urg}
    };
}
