namespace ShiftPlanner.Domain;

public record Schedule
{
    private Schedule(IEnumerable<ShiftAssignment> shifts)
    {
        Shifts = shifts.ToArray();
    }

    public IEnumerable<ShiftAssignment> Shifts { get; private init; }

    public Employee GetAssignedEmployee(Shift shift) =>
        Shifts.Single(s => s.Shift == shift).Employee;

    public static Schedule CreateSchedule(IEnumerable<ShiftAssignment> shifts) => new Schedule(shifts);

    public static readonly Schedule Empty = CreateSchedule(Array.Empty<ShiftAssignment>());
}
