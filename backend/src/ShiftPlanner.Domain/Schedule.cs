namespace ShiftPlanner.Domain;

public record Schedule
{
    private Schedule(IEnumerable<Shift> shifts)
    {
        Shifts = shifts.ToArray();
    }
    
    public IEnumerable<Shift> Shifts { get; private init; }
    
    public Employee GetAssignedEmployee(DateOnly day, Workplace workplace) =>
        Shifts.Single(s => s.Day == day && s.Workplace == workplace).Employee;

    public static Schedule CreateSchedule(IEnumerable<Shift> shifts) => new Schedule(shifts);
}
