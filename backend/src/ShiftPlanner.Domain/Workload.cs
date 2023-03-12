namespace ShiftPlanner.Domain;

public record Workload(uint MinimumShiftsPerMonth, uint MaximumShiftsPerMonth)
{
    public static readonly Workload Infinite = new Workload(0, 999);
    public static Workload None => new Workload(0, 0);
}
