namespace ShiftPlanner.Domain;

public record Leave
{
    public Leave(DateOnly From, DateOnly To)
    {
        if (To < From)
        {
            throw new ArgumentException("End date cannot be before start date", nameof(To));
        }

        this.From = From;
        this.To = To;
    }

    public int LengthDays => (To.ToDateTime(TimeOnly.MinValue) - From.ToDateTime(TimeOnly.MinValue)).Days + 1;
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }

    public void Deconstruct(out DateOnly From, out DateOnly To)
    {
        From = this.From;
        To = this.To;
    }
}
