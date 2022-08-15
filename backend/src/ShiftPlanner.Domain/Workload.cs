namespace ShiftPlanner.Domain;

public record Workload
{
    public static readonly Workload Full = new Workload("1.0", 6, 8);
    
    public static readonly Workload Workload08 = new Workload("0.8", 4, 5);
    public static readonly Workload Workload07 = new Workload("0.7", 4, 5);
    
    public static readonly Workload Workload06 = new Workload("0.6", 3, 4);
    public static readonly Workload Workload05 = new Workload("0.5", 3, 4);
    public static readonly Workload Workload04 = new Workload("0.4", 3, 4);
    
    public static readonly Workload Infinite = new Workload("Infinite", 0, 999);

    private Workload(string name, uint minimumShiftsPerMonth, uint maximumShiftsPerMonth)
    {
        Name = name;
        MinimumShiftsPerMonth = minimumShiftsPerMonth;
        MaximumShiftsPerMonth = maximumShiftsPerMonth;
    }

    public string Name { get; }
    public uint MinimumShiftsPerMonth { get; }
    public uint MaximumShiftsPerMonth { get; }
}
