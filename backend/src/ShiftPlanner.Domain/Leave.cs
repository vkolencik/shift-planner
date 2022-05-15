namespace ShiftPlanner.Domain;

public record Leave(DateOnly From, DateOnly To)
{
    public int LengthDays => throw new NotImplementedException();
}
