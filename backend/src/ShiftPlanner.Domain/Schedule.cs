namespace ShiftPlanner.Domain;

public record Schedule
{
    private Schedule(IEnumerable<ShiftAssignment> shifts)
    {
        Shifts = shifts.ToArray();
        Flexibility = ScheduleFlexibility.Strict;
    }
    
    public IEnumerable<ShiftAssignment> Shifts { get; private init; }
    public ScheduleFlexibility Flexibility { get; private init; } 
    
    public Employee GetAssignedEmployee(Shift shift) =>
        Shifts.Single(s => s.Shift == shift).Employee;

    public static Schedule CreateSchedule(IEnumerable<ShiftAssignment> shifts) => new Schedule(shifts);
}
