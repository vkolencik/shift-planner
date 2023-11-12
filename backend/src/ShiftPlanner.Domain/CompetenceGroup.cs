namespace ShiftPlanner.Domain;

public record CompetenceGroup(IReadOnlyCollection<Workplace> Workplaces)
{
    public static readonly IReadOnlyCollection<CompetenceGroup> CompetenceGroups = new[]
    {
        // new CompetenceGroup(new Workplace[] {Workplace.A1, Workplace.A2, Workplace.A3}),
        // new CompetenceGroup(new Workplace[] {Workplace.Kpr, Workplace.Res, Workplace.Urg}),
        new CompetenceGroup(new Workplace[] {Workplace.A1, Workplace.A2, Workplace.A3}),
        new CompetenceGroup(new Workplace[] {Workplace.Res, Workplace.Urg}),
    };
}
