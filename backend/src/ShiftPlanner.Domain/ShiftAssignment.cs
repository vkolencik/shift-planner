namespace ShiftPlanner.Domain;

public record ShiftAssignment(Shift Shift, Employee Employee)
{
    public ShiftAssignment(Employee employee, Day day, Workplace workplace)
        : this(new Shift(day, workplace), employee)
    {
    }
};
