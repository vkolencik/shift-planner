namespace ShiftPlanner.Domain;

public record PlannedSchedule
{
    private PlannedSchedule(IEnumerable<ShiftAssignment> shifts)
    {
        Shifts = shifts.ToArray();
    }

    public IEnumerable<ShiftAssignment> Shifts { get; private init; }

    public Employee? GetAssignedEmployee(Shift shift) =>
        Shifts.SingleOrDefault(s => s.Shift == shift)?.Employee;

    public static PlannedSchedule CreateSchedule(IEnumerable<ShiftAssignment> shifts) => new PlannedSchedule(shifts);

    public static readonly PlannedSchedule Empty = CreateSchedule(Array.Empty<ShiftAssignment>());
}
